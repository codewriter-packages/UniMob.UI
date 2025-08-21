using System.Collections.Generic;
using UniMob.UI.Layout.Internal.RenderObjects;
using UniMob.UI.Layout.Internal.Views;

namespace UniMob.UI.Layout
{
    public class Row : LayoutWidget, IFlexWidget
    {
        public List<Widget> Children { get; set; } = new();
        public CrossAxisAlignment CrossAxisAlignment { get; set; }
        public MainAxisAlignment MainAxisAlignment { get; set; }
        public AxisSize MainAxisSize { get; set; } = AxisSize.Min;

        public override State CreateState()
        {
            return new RowState();
        }

        public override RenderObject CreateRenderObject(BuildContext context, ILayoutState state)
        {
            return new RenderFlex((RowState) state, Axis.Horizontal);
        }
    }

    public class RowState : LayoutState<Row>, IMultiChildLayoutState
    {
        private readonly StateCollectionHolder _children;

        public RowState()
        {
            _children = CreateChildren(context => Widget.Children);
        }

        public IState[] Children => _children.Value;

        public override WidgetViewReference View => WidgetViewReference.Resource("$$_Layout.MultiChildLayoutView");
    }
}