using UniMob.UI.Layout.Internal.RenderObjects;

namespace UniMob.UI.Layout
{
    public class Align : LayoutWidget
    {
        public Widget Child { get; set; }
        public Alignment Alignment { get; set; } = Alignment.Center;

        public override State CreateState()
        {
            return new AlignState();
        }

        public override RenderObject CreateRenderObject(BuildContext context, ILayoutState state)
        {
            // THIS IS THE FIX: It now creates its own, correct RenderObject.
            return new RenderAlign((AlignState) state);
        }
    }

    public class AlignState : LayoutState<Align>, ISingleChildLayoutState
    {
        private readonly StateHolder _child;

        public AlignState()
        {
            _child = CreateChild(c => Widget.Child);
        }

        public IState Child => _child.Value;

        // The View can still be a simple container that just hosts the child.
        public override WidgetViewReference View => WidgetViewReference.Resource("$$_Layout.AlignView");
    }
}