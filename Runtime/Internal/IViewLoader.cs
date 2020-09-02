namespace UniMob.UI.Internal
{
    public interface IViewLoader
    {
        (IView, WidgetViewReference) LoadViewPrefab(IViewState state);
    }
}