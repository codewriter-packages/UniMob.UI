namespace UniMob.UI.Widgets
{
    public class PaddingBox : SingleChildLayoutWidget
    {
        public PaddingBox(RectPadding padding)
        {
            Padding = padding;
        }

        public RectPadding Padding { get; }

        public override State CreateState() => new PaddingBoxState();
    }

    internal class PaddingBoxState : SingleChildLayoutState<PaddingBox>, IPaddingBoxState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_PaddingBox");

        public Alignment Alignment => Alignment.Center;
        public RectPadding Padding => Widget.Padding;

        public override WidgetSize CalculateSize()
        {
            var padding = Padding;

            var (minW, minH, maxW, maxH) = base.CalculateSize();
            minW += padding.Horizontal;
            maxW += padding.Horizontal;
            minH += padding.Vertical;
            maxH += padding.Vertical;
            return new WidgetSize(minW, minH, maxW, maxH);
        }
    }
}