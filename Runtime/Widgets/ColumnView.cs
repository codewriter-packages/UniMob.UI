using UniMob.UI.Internal;
using UniMob.UI.Widgets;
using UnityEngine;

[assembly: RegisterComponentViewFactory("$$_Column",
    typeof(RectTransform), typeof(ColumnView))]

namespace UniMob.UI.Widgets
{
    internal sealed class ColumnView : View<IColumnState>
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

            var alignX = crossAxis == CrossAxisAlignment.Start ? Alignment.TopLeft.X
                : crossAxis == CrossAxisAlignment.End ? Alignment.TopRight.X
                : Alignment.Center.X;

            var alignY = mainAxis == MainAxisAlignment.Start ? Alignment.TopCenter.Y
                : mainAxis == MainAxisAlignment.End ? Alignment.BottomCenter.Y
                : Alignment.Center.Y;

            var offsetMultiplierY = mainAxis == MainAxisAlignment.Start ? 0
                : mainAxis == MainAxisAlignment.End ? 1f
                : 0.5f;

            var childrenHeight = 0f;
            var unboundedChildCount = 0;

            foreach (var child in children)
            {
                var childHeight = child.Size.GetSizeUnbounded().y;

                if (float.IsInfinity(childHeight))
                {
                    unboundedChildCount += 1;
                }
                else
                {
                    childrenHeight += childHeight;
                }
            }

            var columnHeight = Mathf.Max(Bounds.y, childrenHeight);
            var flexHeight = Mathf.Max(0, (columnHeight - childrenHeight) / Mathf.Max(1, unboundedChildCount));
            var childBound = new Vector2(float.PositiveInfinity, flexHeight);

            var childAlignment = new Alignment(alignX, alignY);
            var corner = childAlignment.WithTop();
            var cornerPosition = new Vector2(0, -childrenHeight * offsetMultiplierY);

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

                    cornerPosition += new Vector2(0, childSize.y);
                }
            }
        }
    }

    internal interface IColumnState : IViewState
    {
        WidgetSize InnerSize { get; }
        IState[] Children { get; }
        CrossAxisAlignment CrossAxisAlignment { get; }
        MainAxisAlignment MainAxisAlignment { get; }
    }
}