namespace UniMob.UI.Widgets
{
    public sealed class Column : MultiChildLayoutWidget
    {
        public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;
        public MainAxisAlignment MainAxisAlignment { get; set; } = MainAxisAlignment.Start;

        public AxisSize CrossAxisSize { get; set; } = AxisSize.Min;
        public AxisSize MainAxisSize { get; set; } = AxisSize.Min;

        public override State CreateState() => new ColumnState();
    }

    internal sealed class ColumnState : MultiChildLayoutState<Column>, IColumnState
    {
        private readonly Atom<WidgetSize> _innerSize;

        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_Column");

        public ColumnState()
        {
            _innerSize = Atom.Computed(CalculateInnerSize);
        }

        public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;
        public MainAxisAlignment MainAxisAlignment => Widget.MainAxisAlignment;
        public WidgetSize InnerSize => _innerSize.Value;

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
                size = size.HasValue ? WidgetSize.StackY(size.Value, child.Size) : child.Size;
            }

            return size.GetValueOrDefault(WidgetSize.Zero);
        }
    }
}