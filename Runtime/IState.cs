using UniMob.UI.Layout;

namespace UniMob.UI
{
    public interface IState
    {
        Key Key { get; }

        Widget RawWidget { get; }

        LayoutConstraints Constraints { get; }

        BuildContext Context { get; }

        IViewState InnerViewState { get; }

        WidgetSize Size { get; }

        Lifetime StateLifetime { get; }

        void UpdateConstraints(LayoutConstraints constraints);
    }
}