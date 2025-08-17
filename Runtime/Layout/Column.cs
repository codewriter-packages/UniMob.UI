using System.Collections.Generic;
using UniMob.UI.Layout.Internal.RenderObjects;
using UniMob.UI.Layout.Views;

namespace UniMob.UI.Layout
{
    public class Column : LayoutWidget, IFlexWidget
    {
        public List<Widget> Children { get; set; } = new ();
        public CrossAxisAlignment CrossAxisAlignment { get; set; }
        public MainAxisAlignment MainAxisAlignment { get; set; }

        public AxisSize MainAxisSize { get; set; } = AxisSize.Min;

        public override State CreateState() => new ColumnState();
        public override RenderObject CreateRenderObject(BuildContext context, ILayoutState state)
        {
            return new RenderFlex(this, (ColumnState)state, Axis.Vertical);
        }
    }
    
    internal class ColumnState : LayoutState<Column>, IMultiChildLayoutState
    {
    private readonly StateCollectionHolder _children;

    public ColumnState()
    {
        _children = CreateChildren(context => Widget.Children);
    }

    
    public IState[] Children => _children.Value;
    public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;
    public MainAxisAlignment MainAxisAlignment => Widget.MainAxisAlignment;
    public WidgetSize InnerSize => default; // Not used by the new system.
    
    public override WidgetViewReference View => WidgetViewReference.Resource("$$_Layout.MultiChildLayoutView");
    }
}