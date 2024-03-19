namespace UniMob.UI.Widgets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Internal;
    using UnityEngine;
    using UnityEngine.UI;

    public class HorizontalScrollGridFlowView : View<IHorizontalScrollGridFlowState>
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
                if (contentPosition > data.cornerPosition.x + data.childSize.x)
                {
                    hidden++;
                }

                if (data.cornerPosition.x < contentPosition + bounds.x)
                {
                    visible++;
                }

                if (sticky != null && data.child.Key == sticky)
                {
                    using (Atom.NoWatch)
                    {
                        var corner = data.cornerPosition.x;
                        var stickToTop = (State.StickyMode & StickyModes.Top) != 0;
                        var stickToBottom = (State.StickyMode & StickyModes.Bottom) != 0;

                        if (stickToTop && contentPosition > corner)
                        {
                            _sticked.Value = StickyModes.Top;
                        }
                        else if (stickToBottom && contentPosition + Bounds.x < corner + data.childSize.x)
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

                contentPosition = -contentRoot.anchoredPosition.x;
                bounds = Bounds;
                hidden = 0;
                visible = 0;
                sticky = State.Sticky;

                DoLayout(State, ContentRender, ChildRender);

                return new Vector2Int(hidden, visible - 1);
            }, debugName: "ScrollGridFlowView._visibilityIndices");
        }

        private void OnContentAnchoredPositionChanged(Vector2 _)
        {
            _scrollValue.Value = (int) -contentRoot.anchoredPosition.x;
            State.ScrollController.NormalizedValue = scroll.horizontalNormalizedPosition;
        }

        protected override void Activate()
        {
            base.Activate();

            if (_mapper == null)
                _mapper = new PooledViewMapper(contentRoot);

            // Sets initial content rectTransform size
            // to prevent unnecessary scrolling in ScrollView
            DoLayout(State, RenderContent);

            scroll.verticalNormalizedPosition = 1f;
            scroll.horizontalNormalizedPosition = State.ScrollController.NormalizedValue;
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
                    layout.Corner = data.childAlignment.WithTop();
                    layout.CornerPosition = data.cornerPosition;

                    Transform childParent = contentRoot;

                    if (isSticky && _sticked.Value is var stickyMode && stickyMode != null)
                    {
                        childParent = contentRoot.parent;

                        switch (stickyMode)
                        {
                            case StickyModes.Top:
                                layout.Corner = layout.Corner.WithLeft();
                                layout.CornerPosition = new Vector2(0, data.cornerPosition.y);
                                break;
                            case StickyModes.Bottom:
                                layout.Corner = layout.Corner.WithRight();
                                layout.CornerPosition = new Vector2(Bounds.x, data.cornerPosition.y);
                                break;
                        }
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
            contentLayout.Size = new Vector2(gridSize.x, float.PositiveInfinity);
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

        private static void DoLayout(IHorizontalScrollGridFlowState state, ContentRenderDelegate renderContentPanel,
            ChildRenderDelegate renderChild = null)
        {
            var children = state.Children;
            var crossAxis = state.CrossAxisAlignment;
            var gridSize = state.InnerSize.GetSizeUnbounded();

            if (float.IsInfinity(gridSize.x))
            {
                foreach (var child in state.Children)
                {
                    if (float.IsInfinity(child.Size.MaxWidth))
                    {
                        Debug.LogError(
                            "Cannot render horizontally stretched widgets inside HorizontalScrollGridFlow.\n" +
                            $"Try to wrap '{child.GetType().Name}' into another widget with fixed width");
                    }
                }

                return;
            }

            var alignY = crossAxis == CrossAxisAlignment.Start ? Alignment.TopLeft.Y
                : crossAxis == CrossAxisAlignment.End ? Alignment.BottomLeft.Y
                : Alignment.Center.Y;

            var alignX = Alignment.CenterLeft.X;

            var offsetMultiplierY = crossAxis == CrossAxisAlignment.Start ? 0.0f
                : crossAxis == CrossAxisAlignment.End ? 1.0f
                : 0.5f;
            
            var childAlignment = new Alignment(alignX, alignY);
            var cornerPosition = new Vector2(0.0f, gridSize.y * offsetMultiplierY);

            // content root
            {
                var contentPivotY = crossAxis == CrossAxisAlignment.Start ? 1.0f
                    : crossAxis == CrossAxisAlignment.End ? 0.0f
                    : 0.5f;

                var contentPivotX = 0.0f;

                var contentPivot = new Vector2(contentPivotX, contentPivotY);
                renderContentPanel(contentPivot, gridSize, childAlignment);
            }

            if (renderChild == null)
            {
                return;
            }

            var newLine = true;
            var lineLastChildIndex = 0;
            var lineWidth = 0f;
            var lineMaxHeight = state.MaxCrossAxisExtent;
            var lineMaxChildCount = state.MaxCrossAxisCount;

            for (var childIndex = 0; childIndex < children.Length; childIndex++)
            {
                var child = children[childIndex];
                var childSize = child.Size.GetSizeUnbounded();

                if (newLine)
                {
                    newLine = false;
                    lineWidth = childSize.x;
                    var lineHeight = childSize.y;

                    if (float.IsInfinity(childSize.y))
                    {
                        if (lineMaxChildCount == 1)
                        {
                            // This is not right, in theory, as it messes with the cornerPosition.x calculation.
                            // However, as long as there is only one child per line, it works.
                            lineHeight = 0;
                        }
                        else
                        {
                            Debug.LogError(
                                $"Cannot render multiple vertically stretched widgets inside HorizontalScrollGridFlow.\n" +
                                $"Try to wrap {child.GetType().Name} into another widget of fixed height or to set {nameof(state.MaxCrossAxisCount)} to 1");
                        }
                    }

                    var lineChildCount = 1;

                    for (int i = childIndex + 1; i < children.Length; i++)
                    {
                        var nextChildSize = children[i].Size.GetSizeUnbounded();
                        if (lineChildCount + 1 <= lineMaxChildCount &&
                            lineHeight + nextChildSize.y <= lineMaxHeight)
                        {
                            lineChildCount++;
                            lineHeight += nextChildSize.y;
                            lineWidth = Math.Max(lineWidth, nextChildSize.x);
                        }
                        else
                        {
                            break;
                        }
                    }

                    lineLastChildIndex = childIndex + lineChildCount - 1;
                    cornerPosition.y = -lineHeight * offsetMultiplierY;
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
                    cornerPosition.x += lineWidth;
                }
                else
                {
                    cornerPosition.y += childSize.y;
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

        public bool ScrollTo(Key key, float duration, float offset, ScrollEasing easing)
        {
            if (!_childPositions.TryGetValue(key, out var position))
            {
                return false;
            }

            var x = position.x - offset;

            // special case for element at end of list
            x -= Mathf.Max(0, Bounds.x - (contentRoot.sizeDelta.x - x));

            // special case for elements at begin of list
            x = Math.Max(0, x);

            StartCoroutine(ScrollTo(Vector2.left * x, duration, easing));
            return true;
        }

        private IEnumerator ScrollTo(Vector2 anchoredPosition, float duration, ScrollEasing easing)
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
                    easing.Invoke(time, duration)
                );

                contentRoot.anchoredPosition = lastAnchoredPosition;
            }

            contentRoot.anchoredPosition = anchoredPosition;
        }
    }

    public interface IHorizontalScrollGridFlowState : IViewState
    {
        WidgetSize InnerSize { get; }
        IState[] Children { get; }
        IState BackgroundContent { get; }
        CrossAxisAlignment CrossAxisAlignment { get; }
        int MaxCrossAxisCount { get; }
        float MaxCrossAxisExtent { get; }
        bool UseMask { get; }
        ScrollController ScrollController { get; }
        Key Sticky { get; }
        StickyModes StickyMode { get; }
    }
}