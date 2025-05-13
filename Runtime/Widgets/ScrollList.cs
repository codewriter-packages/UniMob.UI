namespace UniMob.UI.Widgets
{
    public class ScrollList : MultiChildLayoutWidget
    {
        public static WidgetViewReference DefaultView =
            WidgetViewReference.Resource("UniMob.ScrollList");

        public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;
        public MainAxisAlignment MainAxisAlignment { get; set; } = MainAxisAlignment.Start;
        public bool UseMask { get; set; } = true;
        public MovementType MovementType { get; set; } = MovementType.Elastic;
        public WidgetViewReference View { get; set; } = DefaultView;

        public ScrollController ScrollController { get; set; }

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

        public MovementType MovementType => Widget.MovementType;

        [Atom]
        public ScrollController ScrollController { get; private set; }

        public override void InitState()
        {
            base.InitState();

            ScrollController = Widget.ScrollController ?? new ScrollController(StateLifetime);
        }

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