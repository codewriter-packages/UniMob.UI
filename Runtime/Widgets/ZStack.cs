namespace UniMob.UI.Widgets
{
    public class ZStack : MultiChildLayoutWidget
    {
        public AxisSize CrossAxisSize { get; set; } = AxisSize.Min;
        public AxisSize MainAxisSize { get; set; } = AxisSize.Min;
        
        public Alignment Alignment { get; set; } = Alignment.Center;

        public override State CreateState() => new ZStackState();
    }

    internal class ZStackState : MultiChildLayoutState<ZStack>, IZStackState
    {
        private readonly Atom<WidgetSize> _innerSize;

        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_ZStack");

        public ZStackState()
        {
            _innerSize = Atom.Computed(CalculateInnerSize);
        }

        public Alignment Alignment => Widget.Alignment;

        public override WidgetSize CalculateSize()
        {
            var (minWidth, minHeight, maxWidth, maxHeight) = _innerSize.Value;

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