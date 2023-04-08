using System;
using System.Collections.Generic;
using JetBrains.Annotations;

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

        public override State CreateState() => new GridFlowState();
    }

    [AtomContainer]
    internal class GridFlowState : MultiChildLayoutState<GridFlow>, IGridFlowState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_Grid");

        [Atom] public WidgetSize InnerSize => CalculateInnerSize();
        public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;
        public MainAxisAlignment MainAxisAlignment => Widget.MainAxisAlignment;
        public int MaxCrossAxisCount => Widget.MaxCrossAxisCount;
        public float MaxCrossAxisExtent => Widget.MaxCrossAxisExtent;

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
            var width = 0f;
            var height = 0f;

            var lineWidth = 0.0f;
            var lineHeight = 0.0f;
            var lineChildNum = 0;

            var maxLineWidth = Widget.MaxCrossAxisExtent;
            var maxLineChildNum = Widget.MaxCrossAxisCount;

            foreach (var child in Children)
            {
                var childSize = child.Size;

                if (float.IsInfinity(childSize.MaxWidth) || float.IsInfinity(childSize.MaxHeight))
                {
                    continue;
                }

                if (lineChildNum + 1 <= maxLineChildNum &&
                    lineWidth + childSize.MaxWidth <= maxLineWidth)
                {
                    lineChildNum++;
                    lineWidth += childSize.MaxWidth;
                    lineHeight = Math.Max(lineHeight, childSize.MaxHeight);
                }
                else
                {
                    width = Math.Max(width, lineWidth);
                    height += lineHeight;

                    lineChildNum = 1;
                    lineWidth = childSize.MaxWidth;
                    lineHeight = childSize.MaxHeight;
                }
            }

            width = Math.Max(width, lineWidth);
            height += lineHeight;

            width = Math.Min(width, MaxCrossAxisExtent);

            return WidgetSize.Fixed(width, height);
        }
    }
}