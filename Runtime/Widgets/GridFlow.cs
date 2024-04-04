using UnityEngine;

namespace UniMob.UI.Widgets
{
    public class GridFlow : MultiChildLayoutWidget
    {
        public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;
        public MainAxisAlignment MainAxisAlignment { get; set; } = MainAxisAlignment.Start;
        public AxisSize CrossAxisSize { get; set; } = AxisSize.Min;
        public AxisSize MainAxisSize { get; set; } = AxisSize.Min;
        public RectPadding Padding { get; set; }
        public Vector2 Spacing { get; set; }
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

        public GridLayoutSettings LayoutSettings => new GridLayoutSettings
        {
            children = Children,
            gridPadding = Widget.Padding,
            spacing = Widget.Spacing,
            maxLineWidth = Widget.MaxCrossAxisExtent,
            maxLineChildNum = Widget.MaxCrossAxisCount,
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
            return GridLayoutUtility.CalculateSize(LayoutSettings, LayoutDelegate);
        }
    }
}