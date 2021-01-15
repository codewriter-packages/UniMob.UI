namespace UniMob.UI
{
    public interface IState
    {
        Key Key { get; }

        BuildContext Context { get; }

        IViewState InnerViewState { get; }

        WidgetSize Size { get; }
    }
}