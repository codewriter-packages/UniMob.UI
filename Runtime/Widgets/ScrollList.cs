namespace UniMob.UI.Widgets
{
    public class ScrollList : MultiChildLayoutWidget
    {
        public static WidgetViewReference DefaultView =
            WidgetViewReference.Resource("UniMob.ScrollList");

        public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;
        public MainAxisAlignment MainAxisAlignment { get; set; } = MainAxisAlignment.Start;
        public bool UseMask { get; set; } = true;
        public WidgetViewReference View { get; set; } = DefaultView;

        public override State CreateState() => new ScrollListState();
    }

    public class ScrollListState : MultiChildLayoutState<ScrollList>, IScrollListState
    {
        [Atom] public override WidgetViewReference View => Widget.View;

        public override WidgetSize CalculateSize()
        {
            return WidgetSize.Stretched;
        }

        [Atom] public WidgetSize InnerSize => CalculateInnerSize();
        public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;
        public MainAxisAlignment MainAxisAlignment => Widget.MainAxisAlignment;
        public bool UseMask => Widget.UseMask;

        private WidgetSize CalculateInnerSize()
        {
            var size = WidgetSize.Zero;

            foreach (var child in Children)
            {
                size = WidgetSize.StackY(size, child.Size);
            }

            return size;
        }
    }
}