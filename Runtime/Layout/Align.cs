using UniMob.UI.Layout.Internal.RenderObjects;

namespace UniMob.UI.Layout
{
    public class Align : StatefulWidget
    {
        public Widget Child { get; set; }
        public Alignment Alignment { get; set; } = Alignment.Center;

        public override State CreateState()
        {
            return new AlignState();
        }

        public override RenderObject CreateRenderObject(BuildContext context, IState state)
        {
            return new RenderSizedBox((AlignState) state);
        }
    }
    
    public class AlignState : ViewState<Align>, ISizedBoxState
    {
        private readonly StateHolder _child;

        public float? Width => null;
        public float? Height => null;
        public Alignment Alignment => Widget.Alignment;

        public AlignState()
        {
            _child = CreateChild(c => Widget.Child);
        }

        public IState Child => _child.Value;

        public override WidgetViewReference View => WidgetViewReference.Resource("$$_Layout.AlignView");
    }
}
