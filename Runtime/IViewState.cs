namespace UniMob.UI
{
    public interface IViewState : IState
    {
        WidgetViewReference View { get; }

        void DidViewMount(IView view);
        void DidViewUnmount(IView view);
    }
}