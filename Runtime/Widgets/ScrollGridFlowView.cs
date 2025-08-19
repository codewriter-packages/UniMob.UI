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
            }, debugName: "ScrollGridFlowView._visibilityIndices");
        }

        private void OnContentAnchoredPositionChanged(Vector2 _)
        {
            _scrollValue.Value = (int) contentRoot.anchoredPosition.y;

            if (HasState && !State.StateLifetime.IsDisposed)
            {
                State.ScrollController.NormalizedValue = 1f - scroll.verticalNormalizedPosition;
            }
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
            scroll.verticalNormalizedPosition = 1f - State.ScrollController.NormalizedValue;
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
                    layout.Corner = Alignment.TopLeft;
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
            var crossAxis = state.CrossAxisAlignment;
            var mainAxis = state.MainAxisAlignment;
            var gridSize = state.InnerSize.GetSizeUnbounded();

            var offsetMultiplier = AlignmentUtility.ToOffset(mainAxis, crossAxis);
            var childAlignment = AlignmentUtility.ToAlignment(mainAxis, crossAxis);

            // content root
            {
                var contentPivot = AlignmentUtility.ToPivot(mainAxis, crossAxis);
                renderContentPanel(contentPivot, gridSize, Alignment.TopCenter);
            }

            if (renderChild == null)
            {
                return;
            }

            var settings = state.LayoutSettings;
            var data = GridLayoutUtility.PreLayout(settings);

            while (GridLayoutUtility.LayoutLine(settings, ref data, state.LayoutDelegate))
            {
                var cornerPosition = data.lineContentCornerPosition + new Vector2(
                    -data.lineSize.x * offsetMultiplier.x,
                    -gridSize.y * offsetMultiplier.y);

                for (var i = 0; i < data.lineChildIndex; i++)
                {
                    var childIndex = data.gridChildIndex - data.lineChildIndex + i;
                    var child = settings.children[childIndex];
                    var childSize = child.Size.GetSizeUnbounded();

                    ChildData childData;
                    childData.childIndex = childIndex;
                    childData.child = child;
                    childData.childSize = childSize;
                    childData.childAlignment = childAlignment;
                    childData.cornerPosition = cornerPosition;
                    renderChild(childData);

                    cornerPosition.x += childSize.x + settings.spacing.x;
                }
            }

            GridLayoutUtility.AfterLayout(settings, ref data);
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

        public bool ScrollTo(Key key, float duration, float offset, Easing easing)
        {
            if (!_childPositions.TryGetValue(key, out var position))
            {
                return false;
            }

            var y = position.y - offset;

            // special case for element at end of list
            y -= Mathf.Max(0, Bounds.y - (contentRoot.sizeDelta.y - y));

            // special case for elements at begin of list
            y = Math.Max(0, y);

            StartCoroutine(ScrollTo(Vector2.up * y, duration, easing));
            return true;
        }

        private IEnumerator ScrollTo(Vector2 anchoredPosition, float duration, Easing easing)
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

    public interface IScrollGridFlowState : IViewState
    {
        WidgetSize InnerSize { get; }
        IState[] Children { get; }
        IState BackgroundContent { get; }
        MainAxisAlignment MainAxisAlignment { get; }
        CrossAxisAlignment CrossAxisAlignment { get; }
        bool UseMask { get; }
        ScrollController ScrollController { get; }
        Key Sticky { get; }
        StickyModes StickyMode { get; }
        GridLayoutSettings LayoutSettings { get; }
        GridLayoutDelegate LayoutDelegate { get; }
    }
}