using UniMob.UI.Internal;
using UniMob.UI.Widgets;
using UnityEngine;

[assembly: RegisterComponentViewFactory("$$_Row",
    typeof(RectTransform), typeof(RowView))]

namespace UniMob.UI.Widgets
{
    internal sealed class RowView : View<IRowState>
    {
        private ViewMapperBase _mapper;

        protected override void Activate()
        {
            base.Activate();

            if (_mapper == null)
                _mapper = new PooledViewMapper(transform);
        }

        protected override void Render()
        {
            var children = State.Children;
            var crossAxis = State.CrossAxisAlignment;
            var mainAxis = State.MainAxisAlignment;

            var alignX = mainAxis == MainAxisAlignment.Start ? Alignment.TopLeft.X
                : mainAxis == MainAxisAlignment.End ? Alignment.TopRight.X
                : Alignment.Center.X;

            var alignY = crossAxis == CrossAxisAlignment.Start ? Alignment.TopCenter.Y
                : crossAxis == CrossAxisAlignment.End ? Alignment.BottomCenter.Y
                : Alignment.Center.Y;

            var offsetMultiplierX = mainAxis == MainAxisAlignment.Start ? 0
                : mainAxis == MainAxisAlignment.End ? 1f
                : 0.5f;

            var childrenWidth = 0f;
            var unboundedChildCount = 0;

            foreach (var child in children)
            {
                var childSize = child.Size;

                if (float.IsInfinity(childSize.MaxWidth))
                {
                    unboundedChildCount += 1;
                }
                else
                {
                    childrenWidth += childSize.MaxWidth;
                }
            }

            var rowWidth = Mathf.Max(Bounds.x, childrenWidth);
            var flexWidth = Mathf.Max(0, (rowWidth - childrenWidth) / Mathf.Max(1, unboundedChildCount));
            var childBound = new Vector2(flexWidth, float.PositiveInfinity);

            var childAlignment = new Alignment(alignX, alignY);
            var corner = childAlignment.WithLeft();
            var cornerPosition = new Vector2(-rowWidth * offsetMultiplierX, 0);

            using (var render = _mapper.CreateRender())
            {
                foreach (var child in children)
                {
                    var childSize = child.Size.GetSize(childBound);

                    var childView = render.RenderItem(child);

                    LayoutData layout;
                    layout.Size = childSize;
                    layout.Alignment = childAlignment;
                    layout.Corner = corner;
                    layout.CornerPosition = cornerPosition;
                    ViewLayoutUtility.SetLayout(childView.rectTransform, layout);

                    cornerPosition += new Vector2(childSize.x, 0);
                }
            }
        }
    }

    internal interface IRowState : IViewState
    {
        WidgetSize InnerSize { get; }
        IState[] Children { get; }
        CrossAxisAlignment CrossAxisAlignment { get; }
        MainAxisAlignment MainAxisAlignment { get; }
    }
}