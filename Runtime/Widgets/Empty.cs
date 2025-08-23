using UniMob.UI.Layout.Internal.RenderObjects;

namespace UniMob.UI.Widgets
{
    public class Empty : StatefulWidget
    {
        public override State CreateState() => new EmptyState();

        public override RenderObject CreateRenderObject(BuildContext context, IState state)
        {
            return RenderEmpty.Shared;
        }
    }

    internal class EmptyState : ViewState<Empty>, IEmptyState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_Empty");

        public override WidgetSize CalculateSize() => WidgetSize.Zero;
    }
}