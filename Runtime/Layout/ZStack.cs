using System.Collections.Generic;
using UniMob.UI.Layout.Internal.RenderObjects;
using UniMob.UI.Layout.Views;
using UniMob.UI.Widgets;

namespace UniMob.UI.Layout
{
    public class ZStack : StatefulWidget, IMultiChildLayoutWidget // Reuse IFlexWidget for convenience
    {
        public List<Widget> Children { get; set; } = new List<Widget>();
        public Alignment Alignment { get; set; } = Alignment.Center;

        // Not used by ZStack, but part of the interface
        public MainAxisAlignment MainAxisAlignment => MainAxisAlignment.Start;
        public CrossAxisAlignment CrossAxisAlignment => CrossAxisAlignment.Start;
        public AxisSize MainAxisSize => AxisSize.Min;

        public override State CreateState() => new ZStackState();

        public override RenderObject CreateRenderObject(BuildContext context, IState state)
        {
            return new RenderZStack((ZStackState) state);
        }
    }

    public class ZStackState : ViewState<ZStack>, IMultiChildLayoutState
    {
        private readonly StateCollectionHolder _children;

        public ZStackState()
        {
            _children = CreateChildren(context => Widget.Children);
        }

        public IState[] Children => _children.Value;
        public override WidgetViewReference View => WidgetViewReference.Resource("$$_Layout.MultiChildLayoutView");
    }
}