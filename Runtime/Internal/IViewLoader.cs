namespace UniMob.UI.Internal
{
    public interface IViewLoader
    {
        IView LoadViewPrefab(WidgetViewReference viewReference);
    }
}