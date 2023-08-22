namespace UniMob.UI.Widgets
{
    public class UnPositionedStack : MultiChildLayoutWidget
    {
        public override State CreateState() => new UnPositionedStackState();
    }

    internal class UnPositionedStackState : MultiChildLayoutState<UnPositionedStack>, IUnPositionedStackState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_UnPositionedStack");

        public override WidgetSize CalculateSize()
        {
            return WidgetSize.Stretched;
        }
    }
}