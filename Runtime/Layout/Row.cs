using System.Collections.Generic;

using UniMob.UI.Layout.Internal.RenderObjects;
using UniMob.UI.Layout.Views;

namespace UniMob.UI.Layout
{
    public class Row : LayoutWidget, IFlexWidget
    {
        public List<Widget> Children { get; set; } = new List<Widget>();
        public CrossAxisAlignment CrossAxisAlignment { get; set; }
        public MainAxisAlignment MainAxisAlignment { get; set; } 
        public AxisSize MainAxisSize { get; set; } = AxisSize.Min;

        public override State CreateState() => new RowState();

        public override RenderObject CreateRenderObject(BuildContext context, ILayoutState state)
        {
            return new RenderFlex((RowState) state, Axis.Horizontal);
        }
    }
    
    internal class RowState : LayoutState<Row>, IMultiChildLayoutState
    {
        public IState[] Children => _children.Value;

        private readonly StateCollectionHolder _children;

        public RowState()
        {
            _children = CreateChildren(context => Widget.Children);
        }

        public override WidgetViewReference View => WidgetViewReference.Resource("$$_Layout.MultiChildLayoutView");
    }
}