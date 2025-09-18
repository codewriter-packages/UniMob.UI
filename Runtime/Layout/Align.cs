using UniMob.UI.Layout.Internal.RenderObjects;

namespace UniMob.UI.Layout
{
    public class Align : SingleChildLayoutWidget
    {
        public Alignment Alignment { get; set; } = Alignment.Center;

        public float? WidthFactor { get; set; }
        public float? HeightFactor { get; set; }

        public override State CreateState()
        {
            return new AlignState();
        }

        public override RenderObject CreateRenderObject(BuildContext context, IState state)
        {
            return new RenderPositionedBox((AlignState) state);
        }
    }

    public class AlignState : SingleChildLayoutState<Align>, IPositionedBoxState
    {
        public float? WidthFactor => Widget.WidthFactor;

        public float? HeightFactor => Widget.HeightFactor;

        public Alignment Alignment => Widget.Alignment;
    }
}
