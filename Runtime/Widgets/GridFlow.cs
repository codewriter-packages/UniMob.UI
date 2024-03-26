namespace UniMob.UI.Widgets
{
    public class GridFlow : MultiChildLayoutWidget
    {
        public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;
        public MainAxisAlignment MainAxisAlignment { get; set; } = MainAxisAlignment.Start;
        public AxisSize CrossAxisSize { get; set; } = AxisSize.Min;
        public AxisSize MainAxisSize { get; set; } = AxisSize.Min;
        public int MaxCrossAxisCount { get; set; } = int.MaxValue;
        public float MaxCrossAxisExtent { get; set; } = int.MaxValue;
        public GridLayoutDelegate LayoutDelegate { get; set; }

        public override State CreateState() => new GridFlowState();
    }

    internal class GridFlowState : MultiChildLayoutState<GridFlow>, IGridFlowState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_Grid");

        [Atom] public WidgetSize InnerSize => CalculateInnerSize();
        public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;
        public MainAxisAlignment MainAxisAlignment => Widget.MainAxisAlignment;
        public int MaxCrossAxisCount => Widget.MaxCrossAxisCount;
        public float MaxCrossAxisExtent => Widget.MaxCrossAxisExtent;

        public GridLayoutData LayoutData => new GridLayoutData
        {
            childrenCount = Children.Length,
            maxLineWidth = MaxCrossAxisExtent,
            maxLineChildNum = MaxCrossAxisCount,
        };

        public GridLayoutDelegate LayoutDelegate => Widget.LayoutDelegate ?? GridLayoutUtility.DefaultLayoutDelegate;

        public override WidgetSize CalculateSize()
        {
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
            var data = LayoutData;
            GridLayoutUtility.LayoutGrid(ref data, LayoutDelegate, Children);
            return WidgetSize.Fixed(data.gridWidth, data.gridHeight);
        }
    }
}