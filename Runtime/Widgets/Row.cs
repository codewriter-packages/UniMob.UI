namespace UniMob.UI.Widgets
{
    public sealed class Row : MultiChildLayoutWidget
    {
        public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;
        public MainAxisAlignment MainAxisAlignment { get; set; } = MainAxisAlignment.Start;

        public AxisSize CrossAxisSize { get; set; } = AxisSize.Min;
        public AxisSize MainAxisSize { get; set; } = AxisSize.Min;

        public WidgetSize? Size { get; set; }

        public override State CreateState() => new RowState();
    }

    internal sealed class RowState : MultiChildLayoutState<Row>, IRowState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_Row");

        public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;
        public MainAxisAlignment MainAxisAlignment => Widget.MainAxisAlignment;
        [Atom] public WidgetSize InnerSize => CalculateInnerSize();

        public override WidgetSize CalculateSize()
        {
            if (Widget.Size.HasValue)
            {
                return Widget.Size.Value;
            }
        
            var (minWidth, minHeight, maxWidth, maxHeight) = InnerSize;

            if (Widget.MainAxisSize == AxisSize.Max)
            {
                maxWidth = float.PositiveInfinity;
            }

            if (Widget.CrossAxisSize == AxisSize.Max)
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
                size = size.HasValue ? WidgetSize.StackX(size.Value, child.Size) : child.Size;
            }

            return size.GetValueOrDefault(WidgetSize.Zero);
        }
    }
}