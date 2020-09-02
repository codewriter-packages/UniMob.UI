namespace UniMob.UI
{
    public interface IState
    {
        BuildContext Context { get; }

        IViewState InnerViewState { get; }

        WidgetSize Size { get; }
    }
}