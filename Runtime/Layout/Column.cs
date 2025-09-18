using System.Collections.Generic;
using UniMob.UI.Layout.Internal.RenderObjects;
using UniMob.UI.Layout.Views;

namespace UniMob.UI.Layout
{
    public class Column : StatefulWidget, IFlexWidget
    {
        public List<Widget> Children { get; set; } = new();
        public CrossAxisAlignment CrossAxisAlignment { get; set; }
        public MainAxisAlignment MainAxisAlignment { get; set; }

        public AxisSize MainAxisSize { get; set; } = AxisSize.Min;
        public AxisSize CrossAxisSize { get; set; } = AxisSize.Min;

        public override State CreateState() => new ColumnState();

        public override RenderObject CreateRenderObject(BuildContext context, IState state)
        {
            return new RenderFlex((ColumnState) state, Axis.Vertical);
        }
    }

    internal class ColumnState : ViewState<Column>, IMultiChildLayoutState
    {
        private readonly StateCollectionHolder _children;

        public ColumnState()
        {
            _children = CreateChildren(context => Widget.Children);
        }

        public IState[] Children => _children.Value;

        public override WidgetViewReference View => WidgetViewReference.Resource("$$_Layout.MultiChildLayoutView");
    }
}