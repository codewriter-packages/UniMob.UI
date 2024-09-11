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
            var crossAxis = State.CrossAxisAlignment;
            var mainAxis = State.MainAxisAlignment;
            var gridSize = State.InnerSize.GetSizeUnbounded();

            var offsetMultiplier = AlignmentUtility.ToOffset(mainAxis, crossAxis);
            var childAlignment = AlignmentUtility.ToAlignment(mainAxis, crossAxis);

            using (var render = _mapper.CreateRender())
            {
                var settings = State.LayoutSettings;
                var data = GridLayoutUtility.PreLayout(settings);

                while (GridLayoutUtility.LayoutLine(settings, ref data, State.LayoutDelegate))
                {
                    var cornerPosition = data.lineContentCornerPosition + new Vector2(
                        -data.lineSize.x * offsetMultiplier.x,
                        -gridSize.y * offsetMultiplier.y);

                    for (var i = 0; i < data.lineChildIndex; i++)
                    {
                        var childIndex = data.gridChildIndex - data.lineChildIndex + i;
                        var child = settings.children[childIndex];
                        var childSize = child.Size.GetSizeUnbounded();
                        var childView = render.RenderItem(child);

                        LayoutData layout;
                        layout.Size = childSize;
                        layout.Alignment = childAlignment;
                        layout.Corner = Alignment.TopLeft;
                        layout.CornerPosition = cornerPosition;
                        ViewLayoutUtility.SetLayout(childView.rectTransform, layout);

                        cornerPosition.x += childSize.x + settings.spacing.x;
                    }
                }

                GridLayoutUtility.AfterLayout(settings, ref data);
            }
        }
    }

    internal interface IGridFlowState : IViewState
    {
        WidgetSize InnerSize { get; }
        CrossAxisAlignment CrossAxisAlignment { get; }
        MainAxisAlignment MainAxisAlignment { get; }

        GridLayoutSettings LayoutSettings { get; }
        GridLayoutDelegate LayoutDelegate { get; }
    }
}