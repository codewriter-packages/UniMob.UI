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
        public bool Draggable {get; set;} = true; 
        public AxisSize CrossAxisSize { get; set; } = AxisSize.Min;
        public AxisSize MainAxisSize { get; set; } = AxisSize.Min;
        public WidgetSize? Size { get; set; }

        public override State CreateState() => new TabsState();
    }

    public class TabsState : MultiChildLayoutState<Tabs>, ITabsState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("UniMob.Tabs");

        public TabController TabController => Widget.TabController;
        public bool UseMask => Widget.UseMask;

        [Atom] public WidgetSize InnerSize => CalculateInnerSize();
        [Atom] public bool Draggable => Widget.Draggable;

        public override WidgetSize CalculateSize()
        {
            if (Widget.Size.HasValue)
            {
                return Widget.Size.Value;
            }

            var (minWidth, minHeight, maxWidth, maxHeight) = InnerSize;

            if (Widget.CrossAxisSize == AxisSize.Max)
            {
                maxWidth = float.PositiveInfinity;
            }

            if (Widget.MainAxisSize == AxisSize.Max)
            {
                maxHeight = float.PositiveInfinity;
            }

            return new WidgetSize(minWidth, minHeight, maxWidth, maxHeight);
        }

        private WidgetSize CalculateInnerSize()
        {
            var size = default(WidgetSize?);

            foreach (var child in Children)
            {
                size = size.HasValue ? WidgetSize.StackZ(size.Value, child.Size) : child.Size;
            }

            return size.GetValueOrDefault(WidgetSize.Zero);
        }
    }
}