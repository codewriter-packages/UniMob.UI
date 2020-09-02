using UniMob.UI.Internal;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    internal class ScrollListView : View<IScrollListState>
    {
        private RectTransform _contentRoot;
        private ViewMapperBase _mapper;

        protected override void Awake()
        {
            base.Awake();

            _contentRoot = (RectTransform) transform.GetChild(0);
        }

        protected override void Activate()
        {
            base.Activate();

            if (_mapper == null)
                _mapper = new PooledViewMapper(_contentRoot);
        }

        protected override void Render()
        {
            var children = State.Children;
            var mainAxis = State.MainAxisAlignment;
            var crossAxis = State.CrossAxisAlignment;
            var listSize = State.InnerSize;

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

                _contentRoot.pivot = new Vector2(contentPivotX, contentPivotY);

                LayoutData contentLayout;
                contentLayout.Size = WidgetSize.FixedHeight(listSize.Height);
                contentLayout.Alignment = childAlignment;
                contentLayout.Corner = childAlignment;
                contentLayout.CornerPosition = Vector2.zero;
                ViewLayoutUtility.SetLayout(_contentRoot, contentLayout);
            }

            using (var render = _mapper.CreateRender())
            {
                foreach (var child in children)
                {
                    var childSize = child.Size;

                    if (childSize.IsHeightStretched)
                    {
                        Debug.LogError("Cannot render vertically stretched widgets inside ScrollList.\n" +
                                       $"Try to wrap '{child.GetType().Name}' into another widget with fixed height");
                        continue;
                    }

                    var childView = render.RenderItem(child);

                    LayoutData layout;
                    layout.Size = childSize;
                    layout.Alignment = childAlignment;
                    layout.Corner = corner;
                    layout.CornerPosition = cornerPosition;
                    ViewLayoutUtility.SetLayout(childView.rectTransform, layout);

                    cornerPosition += new Vector2(0, childSize.Height);
                }
            }
        }
    }

    internal interface IScrollListState : IViewState
    {
        WidgetSize InnerSize { get; }
        IState[] Children { get; }
        CrossAxisAlignment CrossAxisAlignment { get; }
        MainAxisAlignment MainAxisAlignment { get; }
    }
}