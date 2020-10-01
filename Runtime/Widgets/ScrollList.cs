namespace UniMob.UI.Widgets
{
    public class ScrollList : MultiChildLayoutWidget
    {
        public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;
        public MainAxisAlignment MainAxisAlignment { get; set; } = MainAxisAlignment.Start;

        public override State CreateState() => new ScrollListState();
    }

    internal class ScrollListState : MultiChildLayoutState<ScrollList>, IScrollListState
    {
        private readonly Atom<WidgetSize> _innerSize;

        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("UniMob.ScrollList");

        public ScrollListState()
        {
            _innerSize = Atom.Computed(CalculateInnerSize);
        }

        public override WidgetSize CalculateSize()
        {
            return WidgetSize.Stretched;
        }

        public WidgetSize InnerSize => _innerSize.Value;
        public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;
        public MainAxisAlignment MainAxisAlignment => Widget.MainAxisAlignment;

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