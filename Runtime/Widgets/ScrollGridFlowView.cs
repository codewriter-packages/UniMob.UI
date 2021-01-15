namespace UniMob.UI.Widgets
{
    using System;
    using Internal;
    using UnityEngine;
    using UnityEngine.UI;

    public class ScrollGridFlowView : View<IScrollGridFlowState>
    {
        private RectTransform _contentRoot;

        private ScrollRect _scroll;

        private ViewMapperBase _mapper;

        private readonly MutableAtom<int> _firstVisibleChildIndex = Atom.Value(0);
        private readonly MutableAtom<int> _lastVisibleChildIndex = Atom.Value(0);
        private readonly MutableAtom<int> _scrollValue = Atom.Value(0);

        private Reaction _countersUpdater;

        protected override void Awake()
        {
            base.Awake();

            _contentRoot = (RectTransform) transform.GetChild(0);

            _scroll = GetComponent<ScrollRect>();
            _scroll.onValueChanged.AddListener(OnScrollChanged);

            _countersUpdater = new ReactionAtom(null, () =>
            {
                _scrollValue.Get();

                int hidden = 0, visible = 0;
                var bounds = Bounds;
                var pos = _contentRoot.anchoredPosition.y;

                DoLayout(State,
                    renderContentPanel: (contentPivot, gridSize, childAlignment) => { },
                    renderChild: (childIndex, child, childSize, childAlignment, cornerPosition) =>
                    {
                        if (pos > cornerPosition.y + childSize.y)
                        {
                            hidden++;
                        }

                        if (cornerPosition.y < pos + bounds.y)
                        {
                            visible++;
                        }
                    }
                );

                _firstVisibleChildIndex.Value = hidden;
                _lastVisibleChildIndex.Value = visible - 1;
            });
        }

        private void OnScrollChanged(Vector2 value)
        {
            _scrollValue.Value = (int) _contentRoot.anchoredPosition.y;
        }

        protected override void Activate()
        {
            base.Activate();

            if (_mapper == null)
                _mapper = new PooledViewMapper(_contentRoot);

            _countersUpdater.Activate();
        }

        protected override void Deactivate()
        {
            _countersUpdater.Deactivate();

            base.Deactivate();
        }

        protected override void Render()
        {
            var startIndex = _firstVisibleChildIndex.Value;
            var endIndex = _lastVisibleChildIndex.Value;

            using (var render = _mapper.CreateRender())
            {
                DoLayout(State,
                    renderContentPanel: (contentPivot, gridSize, childAlignment) =>
                    {
                        _contentRoot.pivot = contentPivot;

                        LayoutData contentLayout;
                        contentLayout.Size = new Vector2(float.PositiveInfinity, gridSize.y);
                        contentLayout.Alignment = childAlignment;
                        contentLayout.Corner = childAlignment;
                        contentLayout.CornerPosition = null;
                        ViewLayoutUtility.SetLayout(_contentRoot, contentLayout);
                    },
                    renderChild: (childIndex, child, childSize, childAlignment, cornerPosition) =>
                    {
                        if (childIndex < startIndex || childIndex > endIndex)
                        {
                            return;
                        }

                        // ReSharper disable once AccessToDisposedClosure
                        var childView = render.RenderItem(child);

                        LayoutData layout;
                        layout.Size = childSize;
                        layout.Alignment = childAlignment;
                        layout.Corner = childAlignment.WithLeft();
                        layout.CornerPosition = cornerPosition;
                        ViewLayoutUtility.SetLayout(childView.rectTransform, layout);
                    }
                );
            }
        }

        private static void DoLayout(IScrollGridFlowState state, ContentRenderDelegate renderContentPanel,
            ChildRenderDelegate renderChild)
        {
            var children = state.Children;
            var crossAxis = state.CrossAxisAlignment;
            var mainAxis = state.MainAxisAlignment;
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

            var alignY = mainAxis == MainAxisAlignment.Start ? Alignment.TopCenter.Y
                : mainAxis == MainAxisAlignment.End ? Alignment.BottomCenter.Y
                : Alignment.Center.Y;

            var offsetMultiplierX = crossAxis == CrossAxisAlignment.Start ? 0.0f
                : crossAxis == CrossAxisAlignment.End ? 1.0f
                : 0.5f;

            var offsetMultiplierY = mainAxis == MainAxisAlignment.Start ? 0.0f
                : mainAxis == MainAxisAlignment.End ? 1.0f
                : 0.5f;

            var childAlignment = new Alignment(alignX, alignY);
            var cornerPosition = new Vector2(
                -gridSize.x * offsetMultiplierX,
                -gridSize.y * offsetMultiplierY);

            // content root
            {
                var contentPivotX = crossAxis == CrossAxisAlignment.Start ? 0.0f
                    : crossAxis == CrossAxisAlignment.End ? 1.0f
                    : 0.5f;

                var contentPivotY = mainAxis == MainAxisAlignment.Start ? 1.0f
                    : mainAxis == MainAxisAlignment.End ? 0.0f
                    : 0.5f;

                var contentPivot = new Vector2(contentPivotX, contentPivotY);
                renderContentPanel(contentPivot, gridSize, childAlignment);
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

                renderChild(childIndex, child, childSize, childAlignment,
                    cornerPosition + new Vector2(0, lineHeight * offsetMultiplierY));

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

        private delegate void ChildRenderDelegate(int childIndex, IState child, Vector2 childSize,
            Alignment childAlignment, Vector2 connerPosition);
    }

    public interface IScrollGridFlowState : IViewState
    {
        WidgetSize InnerSize { get; }
        IState[] Children { get; }
        CrossAxisAlignment CrossAxisAlignment { get; }
        MainAxisAlignment MainAxisAlignment { get; }
        int MaxCrossAxisCount { get; }
        float MaxCrossAxisExtent { get; }
    }
}