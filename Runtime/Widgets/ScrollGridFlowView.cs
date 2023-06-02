namespace UniMob.UI.Widgets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Internal;
    using UnityEngine;
    using UnityEngine.UI;

    public class ScrollGridFlowView : View<IScrollGridFlowState>
    {
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private RectMask2D rectMask;
        [SerializeField] private ScrollRect scroll;

        private ViewMapperBase _mapper;

        private readonly MutableAtom<int> _scrollValue = Atom.Value(0);
        private readonly MutableAtom<StickyModes?> _sticked = Atom.Value<StickyModes?>(null);
        private readonly List<ChildData> _nextChildren = new List<ChildData>();

        private Atom<Vector2Int> _visibilityIndices;

        private readonly Dictionary<Key, Vector2> _childPositions = new Dictionary<Key, Vector2>();

        protected override void Awake()
        {
            base.Awake();

            scroll.onValueChanged.Bind(OnContentAnchoredPositionChanged);

            SetupVirtualization();
        }

        private void SetupVirtualization()
        {
            var contentPosition = 0f;
            var bounds = Vector2Int.zero;
            var hidden = 0;
            var visible = 0;
            var sticky = default(Key);

            void ContentRender(Vector2 contentPivot, Vector2 gridSize, Alignment childAlignment)
            {
            }

            void ChildRender(ChildData data)
            {
                if (contentPosition > data.cornerPosition.y + data.childSize.y)
                {
                    hidden++;
                }

                if (data.cornerPosition.y < contentPosition + bounds.y)
                {
                    visible++;
                }

                if (sticky != null && data.child.Key == sticky)
                {
                    using (Atom.NoWatch)
                    {
                        var corner = data.cornerPosition.y;
                        var stickToTop = (State.StickyMode & StickyModes.Top) != 0;
                        var stickToBottom = (State.StickyMode & StickyModes.Bottom) != 0;

                        if (stickToTop && contentPosition > corner)
                        {
                            _sticked.Value = StickyModes.Top;
                        }
                        else if (stickToBottom && contentPosition + Bounds.y < corner + data.childSize.y)
                        {
                            _sticked.Value = StickyModes.Bottom;
                        }
                        else
                        {
                            _sticked.Value = null;
                        }
                    }
                }
            }

            _visibilityIndices = Atom.Computed(ViewLifetime, () =>
            {
                _scrollValue.Get();

                contentPosition = contentRoot.anchoredPosition.y;
                bounds = Bounds;
                hidden = 0;
                visible = 0;
                sticky = State.Sticky;

                DoLayout(State, ContentRender, ChildRender);

                return new Vector2Int(hidden, visible - 1);
            });
        }

        private void OnContentAnchoredPositionChanged(Vector2 _)
        {
            _scrollValue.Value = (int) contentRoot.anchoredPosition.y;
        }

        protected override void Activate()
        {
            base.Activate();

            if (_mapper == null)
                _mapper = new PooledViewMapper(contentRoot);

            // Sets initial content rectTransform size
            // to prevent unnecessary scrolling in ScrollView
            DoLayout(State, RenderContent);

            scroll.horizontalNormalizedPosition = 0f;
            scroll.verticalNormalizedPosition = 1f;
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            _childPositions.Clear();
            _nextChildren.Clear();
            _visibilityIndices.Deactivate();
        }

        protected override void Render()
        {
            var sticky = State.Sticky;
            var useMask = State.UseMask;
            if (rectMask.enabled != useMask)
            {
                rectMask.enabled = useMask;
            }

            var visibilityIndices = _visibilityIndices.Value;
            var startIndex = visibilityIndices.x;
            var endIndex = visibilityIndices.y;

            _childPositions.Clear();
            _nextChildren.Clear();

            using (var render = _mapper.CreateRender())
            {
                if (State.BackgroundContent is var backgroundContentState &&
                    !(backgroundContentState is EmptyState))
                {
                    var backgroundContentView = render.RenderItem(backgroundContentState);
                    backgroundContentView.rectTransform.SetSiblingIndex(0);

                    var childSize = backgroundContentState.Size;

                    LayoutData layout;
                    layout.Size = childSize.GetSizeUnbounded();
                    layout.Alignment = Alignment.Center;
                    layout.Corner = Alignment.Center;
                    layout.CornerPosition = Vector2.zero;
                    ViewLayoutUtility.SetLayout(backgroundContentView.rectTransform, layout);
                }

                DoLayout(State, RenderContent, RenderChild);

                foreach (var data in _nextChildren)
                {
                    if (data.childIndex >= startIndex && data.childIndex <= endIndex)
                    {
                        continue;
                    }

                    render.Reuse(data.child);
                }

                foreach (var data in _nextChildren)
                {
                    var isSticky = data.child.Key != null && data.child.Key == sticky;

                    if ((data.childIndex < startIndex || data.childIndex > endIndex) && !isSticky)
                    {
                        continue;
                    }

                    var childView = render.RenderItem(data.child);

                    LayoutData layout;
                    layout.Size = data.childSize;
                    layout.Alignment = data.childAlignment;
                    layout.Corner = data.childAlignment.WithLeft();
                    layout.CornerPosition = data.cornerPosition;

                    Transform childParent = contentRoot;

                    if (isSticky && _sticked.Value is var stickyMode && stickyMode != null)
                    {
                        childParent = contentRoot.parent;

                        layout.Corner = stickyMode == StickyModes.Top
                            ? Alignment.TopCenter
                            : Alignment.BottomCenter;

                        layout.CornerPosition = stickyMode == StickyModes.Top
                            ? Vector2.zero
                            : new Vector2(0, Bounds.y);
                    }

                    if (childView.rectTransform.parent != childParent)
                    {
                        childView.rectTransform.SetParent(childParent, false);
                    }

                    ViewLayoutUtility.SetLayout(childView.rectTransform, layout);
                }
            }
        }

        private void RenderContent(Vector2 contentPivot, Vector2 gridSize, Alignment childAlignment)
        {
            contentRoot.pivot = contentPivot;

            LayoutData contentLayout;
            contentLayout.Size = new Vector2(float.PositiveInfinity, gridSize.y);
            contentLayout.Alignment = childAlignment;
            contentLayout.Corner = childAlignment;
            contentLayout.CornerPosition = null;
            ViewLayoutUtility.SetLayout(contentRoot, contentLayout);
        }

        private void RenderChild(ChildData data)
        {
            if (data.child.Key != null)
            {
                _childPositions.Add(data.child.Key, data.cornerPosition);
            }

            _nextChildren.Add(data);
        }

        private static void DoLayout(IScrollGridFlowState state, ContentRenderDelegate renderContentPanel,
            ChildRenderDelegate renderChild = null)
        {
            var children = state.Children;
            var crossAxis = state.CrossAxisAlignment;
            var gridSize = state.InnerSize.GetSizeUnbounded();

            if (float.IsInfinity(gridSize.y))
            {
                foreach (var child in state.Children)
                {
                    if (float.IsInfinity(child.Size.MaxHeight))
                    {
                        Debug.LogError("Cannot render vertically stretched widgets inside ScrollGridFlow.\n" +
                                       $"Try to wrap '{child.GetType().Name}' into another widget with fixed height");
                    }
                }

                return;
            }

            var alignX = crossAxis == CrossAxisAlignment.Start ? Alignment.TopLeft.X
                : crossAxis == CrossAxisAlignment.End ? Alignment.TopRight.X
                : Alignment.Center.X;

            var alignY = Alignment.TopCenter.Y;

            var offsetMultiplierX = crossAxis == CrossAxisAlignment.Start ? 0.0f
                : crossAxis == CrossAxisAlignment.End ? 1.0f
                : 0.5f;


            var childAlignment = new Alignment(alignX, alignY);
            var cornerPosition = new Vector2(-gridSize.x * offsetMultiplierX, 0.0f);

            // content root
            {
                var contentPivotX = crossAxis == CrossAxisAlignment.Start ? 0.0f
                    : crossAxis == CrossAxisAlignment.End ? 1.0f
                    : 0.5f;

                var contentPivotY = 1.0f;

                var contentPivot = new Vector2(contentPivotX, contentPivotY);
                renderContentPanel(contentPivot, gridSize, childAlignment);
            }

            if (renderChild == null)
            {
                return;
            }

            var newLine = true;
            var lineLastChildIndex = 0;
            var lineHeight = 0f;
            var lineMaxWidth = state.MaxCrossAxisExtent;
            var lineMaxChildCount = state.MaxCrossAxisCount;

            for (var childIndex = 0; childIndex < children.Length; childIndex++)
            {
                var child = children[childIndex];
                var childSize = child.Size.GetSizeUnbounded();

                if (newLine)
                {
                    newLine = false;
                    lineHeight = childSize.y;
                    var lineWidth = childSize.x;

                    if (float.IsInfinity(childSize.x))
                    {
                        if (lineMaxChildCount == 1)
                        {
                            // This is not right, in theory, as it messes with the cornerPosition.x calculation.
                            // However, as long as there is only one child per line, it works.
                            lineWidth = 0;
                        }
                        else
                        {
                            Debug.LogError(
                                $"Cannot render multiple horizontally stretched widgets inside ScrollGridFlow.\n" +
                                $"Try to wrap {child.GetType().Name} into another widget of fixed width or to set {nameof(state.MaxCrossAxisCount)} to 1");
                        }
                    }

                    var lineChildCount = 1;

                    for (int i = childIndex + 1; i < children.Length; i++)
                    {
                        var nextChildSize = children[i].Size.GetSizeUnbounded();
                        if (lineChildCount + 1 <= lineMaxChildCount &&
                            lineWidth + nextChildSize.x <= lineMaxWidth)
                        {
                            lineChildCount++;
                            lineWidth += nextChildSize.x;
                            lineHeight = Math.Max(lineHeight, nextChildSize.y);
                        }
                        else
                        {
                            break;
                        }
                    }

                    lineLastChildIndex = childIndex + lineChildCount - 1;
                    cornerPosition.x = -lineWidth * offsetMultiplierX;
                }

                ChildData data;
                data.childIndex = childIndex;
                data.child = child;
                data.childSize = childSize;
                data.childAlignment = childAlignment;
                data.cornerPosition = cornerPosition;
                renderChild(data);

                if (childIndex == lineLastChildIndex)
                {
                    newLine = true;
                    cornerPosition.y += lineHeight;
                }
                else
                {
                    cornerPosition.x += childSize.x;
                }
            }
        }

        private delegate void ContentRenderDelegate(Vector2 contentPivot, Vector2 gridSize, Alignment childAlignment);

        private delegate void ChildRenderDelegate(ChildData data);

        [Serializable]
        private struct ChildData
        {
            public int childIndex;
            public IState child;
            public Vector2 childSize;
            public Alignment childAlignment;
            public Vector2 cornerPosition;
        }

        public bool ScrollTo(Key key, float duration, float offset)
        {
            if (!_childPositions.TryGetValue(key, out var position))
            {
                return false;
            }

            var y = position.y - offset;

            // special case for element at end of list
            y -= Mathf.Max(0, Bounds.y - (contentRoot.sizeDelta.y - y));

            StartCoroutine(ScrollTo(Vector2.up * y, duration));
            return true;
        }

        private IEnumerator ScrollTo(Vector2 anchoredPosition, float duration)
        {
            var time = 0f;

            var lastAnchoredPosition = contentRoot.anchoredPosition;
            while (time < duration)
            {
                yield return null;

                if (Vector2.SqrMagnitude(lastAnchoredPosition - contentRoot.anchoredPosition) > 0.1f)
                {
                    yield break;
                }

                time += Time.unscaledDeltaTime;

                lastAnchoredPosition = Vector2.LerpUnclamped(
                    contentRoot.anchoredPosition,
                    anchoredPosition,
                    CircEaseInOut(time, duration)
                );

                contentRoot.anchoredPosition = lastAnchoredPosition;
            }

            contentRoot.anchoredPosition = anchoredPosition;
        }

        public static float CircEaseInOut(float t, float d)
        {
            if ((t /= d / 2) < 1)
                return -0.5f * (Mathf.Sqrt(1 - t * t) - 1);

            return 0.5f * (Mathf.Sqrt(1 - (t -= 2) * t) + 1);
        }
    }

    public interface IScrollGridFlowState : IViewState
    {
        WidgetSize InnerSize { get; }
        IState[] Children { get; }
        IState BackgroundContent { get; }
        CrossAxisAlignment CrossAxisAlignment { get; }
        int MaxCrossAxisCount { get; }
        float MaxCrossAxisExtent { get; }
        bool UseMask { get; }
        Key Sticky { get; }
        StickyModes StickyMode { get; }
    }
}