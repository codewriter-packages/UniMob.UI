namespace UniMob.UI.Widgets
{
    public class Tabs : MultiChildLayoutWidget
    {
        public Tabs(TabController tabController)
        {
            TabController = tabController;
        }

        public TabController TabController { get; }

        public bool UseMask { get; set; } = true;

        public override State CreateState() => new TabsState();
    }

    public class TabsState : MultiChildLayoutState<Tabs>, ITabsState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("UniMob.Tabs");

        public TabController TabController => Widget.TabController;
        public bool UseMask => Widget.UseMask;

        public override WidgetSize CalculateSize()
        {
            return WidgetSize.Stretched;
        }
    }
}