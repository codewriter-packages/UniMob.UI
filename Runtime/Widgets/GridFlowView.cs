using UniMob.UI.Internal;
using UniMob.UI.Widgets;
using UnityEngine;

[assembly: RegisterComponentViewFactory("$$_Grid", typeof(RectTransform), typeof(GridFlowView))]

namespace UniMob.UI.Widgets
{
    internal class GridFlowView : View<IGridFlowState>
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
            var gridSize = State.InnerSize.GetSize(Bounds);

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

            using (var render = _mapper.CreateRender())
            {
                var data = State.LayoutData;

                var startLineChildIndex = 0;

                while (GridLayoutUtility.LayoutLine(ref data, State.LayoutDelegate, children, startLineChildIndex,
                    out var lastLineChildIndex))
                {
                    cornerPosition.x = -data.lineWidth * offsetMultiplierX;

                    for (var childIndex = startLineChildIndex; childIndex <= lastLineChildIndex; childIndex++)
                    {
                        var child = children[childIndex];
                        var childSize = child.Size.GetSize(Bounds);
                        var childView = render.RenderItem(child);

                        LayoutData layout;
                        layout.Size = childSize;
                        layout.Alignment = childAlignment;
                        layout.Corner = childAlignment.WithLeft();
                        layout.CornerPosition = cornerPosition + new Vector2(0, data.lineHeight * offsetMultiplierY);
                        ViewLayoutUtility.SetLayout(childView.rectTransform, layout);

                        cornerPosition.x += childSize.x;
                    }

                    cornerPosition.y += data.lineHeight;

                    startLineChildIndex = lastLineChildIndex + 1;
                }
            }
        }
    }

    internal interface IGridFlowState : IViewState
    {
        WidgetSize InnerSize { get; }
        IState[] Children { get; }
        CrossAxisAlignment CrossAxisAlignment { get; }
        MainAxisAlignment MainAxisAlignment { get; }
        int MaxCrossAxisCount { get; }
        float MaxCrossAxisExtent { get; }

        GridLayoutData LayoutData { get; }
        GridLayoutDelegate LayoutDelegate { get; }
    }
}