using System;
using UniMob.UI.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace UniMob.UI.Widgets
{
    internal class ScrollListView : View<IScrollListState>
    {
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private RectMask2D rectMask;
        [SerializeField] private ScrollRect scroll;

        private float? _previousHeight;
        private ViewMapperBase _mapper;

        protected override void Awake()
        {
            base.Awake();
            scroll.onValueChanged.Bind(OnContentAnchoredPositionChanged);
        }

        private void OnContentAnchoredPositionChanged(Vector2 _)
        {
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

            scroll.horizontalNormalizedPosition = 0f;
            scroll.verticalNormalizedPosition = 1f - State.ScrollController.NormalizedValue;
        }

        protected override void Render()
        {
            var useMask = State.UseMask;
            if (rectMask.enabled != useMask)
            {
                rectMask.enabled = useMask;
            }
            if (((int) scroll.movementType) != (int) State.MovementType)
            {
                scroll.movementType = State.MovementType switch
                {
                    MovementType.Clamped => ScrollRect.MovementType.Clamped,
                    _ => ScrollRect.MovementType.Elastic,
                };
            }
            var children = State.Children;
            var mainAxis = State.MainAxisAlignment;
            var crossAxis = State.CrossAxisAlignment;
            var listSize = State.InnerSize.GetSizeUnbounded();

            if (float.IsInfinity(listSize.y))
            {
                foreach (var child in State.Children)
                {
                    if (float.IsInfinity(child.Size.MaxHeight))
                    {
                        Debug.LogError("Cannot render vertically stretched widgets inside ScrollList.\n" +
                                       $"Try to wrap '{child.GetType().Name}' into another widget with fixed height");
                    }
                }

                return;
            }

            // save the current size to maybe restore the scroll position
            _previousHeight = listSize.y;

            var alignX = crossAxis == CrossAxisAlignment.Start ? Alignment.TopLeft.X
                : crossAxis == CrossAxisAlignment.End ? Alignment.TopRight.X
                : Alignment.Center.X;

            var childAlignment = new Alignment(alignX, Alignment.TopCenter.Y);
            var corner = childAlignment.WithTop();
            var cornerPosition = new Vector2(0, 0);

            // content root
            {
                var contentPivotX = crossAxis == CrossAxisAlignment.Start ? 0.0f
                    : crossAxis == CrossAxisAlignment.End ? 1.0f
                    : 0.5f;

                var contentPivotY = mainAxis == MainAxisAlignment.Start ? 1.0f
                    : mainAxis == MainAxisAlignment.End ? 0.0f
                    : 0.5f;

                contentRoot.pivot = new Vector2(contentPivotX, contentPivotY);

                LayoutData contentLayout;
                contentLayout.Size = new Vector2(float.PositiveInfinity, listSize.y);
                contentLayout.Alignment = childAlignment;
                contentLayout.Corner = childAlignment;
                contentLayout.CornerPosition = Vector2.zero;
                ViewLayoutUtility.SetLayout(contentRoot, contentLayout);
            }

            using (var render = _mapper.CreateRender())
            {
                foreach (var child in children)
                {
                    var childSize = child.Size.GetSizeUnbounded();

                    var childView = render.RenderItem(child);

                    LayoutData layout;
                    layout.Size = childSize;
                    layout.Alignment = childAlignment;
                    layout.Corner = corner;
                    layout.CornerPosition = cornerPosition;
                    ViewLayoutUtility.SetLayout(childView.rectTransform, layout);

                    cornerPosition += new Vector2(0, childSize.y);
                }
            }

            // here we _maybe_ restore scroll size. We only do that if the height of the list is preserved
            if (_previousHeight.HasValue && Mathf.Approximately(listSize.y, _previousHeight.Value))
                scroll.verticalNormalizedPosition = 1 - State.ScrollController.NormalizedValue;
        }
    }

    internal interface IScrollListState : IViewState
    {
        ScrollController ScrollController { get; }

        WidgetSize InnerSize { get; }
        IState[] Children { get; }
        CrossAxisAlignment CrossAxisAlignment { get; }
        MainAxisAlignment MainAxisAlignment { get; }
        bool UseMask { get; }
        MovementType MovementType { get; }
    }
}