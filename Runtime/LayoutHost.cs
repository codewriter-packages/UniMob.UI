using System;
using UniMob.UI;
using UniMob.UI.Layout;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    /// <summary>
    /// This widget hosts a LayoutWidget inside a legacy parent container
    /// It correctly translates size information between the two systems.
    /// </summary>
    public class LayoutHost : StatefulWidget
    {
        public Widget Child { get; set; }

        public AxisSize AxisSize { get; set; } = AxisSize.Min;

        public override State CreateState() => new LayoutHostState();
    }

    // The State for the LayoutHost.
    internal class LayoutHostState : ViewState<LayoutHost>
    {
        private readonly StateHolder _child;
        public IState Child => _child.Value;
        private ILayoutState LayoutChild => _child.Value as ILayoutState;

        public LayoutHostState()
        {
            _child = CreateChild(context => Widget.Child);
        }

        public override WidgetSize CalculateSize()
        {
            if (LayoutChild is null)
                throw new InvalidOperationException($"{nameof(Child)} must implement {nameof(ILayoutState)}");
            var ro = LayoutChild.RenderObject;
            if (ro == null)
            {
                return WidgetSize.Zero;
            }


            var intrinsicWidth = ro.GetIntrinsicWidth(float.PositiveInfinity);
            var intrinsicHeight = ro.GetIntrinsicHeight(float.PositiveInfinity);

            var minWidth = float.IsInfinity(intrinsicWidth) ? 0 : intrinsicWidth;
            var maxWidth = float.IsInfinity(intrinsicWidth) ? float.PositiveInfinity : intrinsicWidth;

            var minHeight = float.IsInfinity(intrinsicHeight) ? 0 : intrinsicHeight;
            var maxHeight = float.IsInfinity(intrinsicHeight) ? float.PositiveInfinity : intrinsicHeight;

            var tightSize = new WidgetSize(minWidth, minHeight, maxWidth, maxHeight);

            if (Widget.AxisSize == AxisSize.Min) return tightSize;
            
            // Traverse the tree upwards to find whether we are in a row or column.
            // Check if they offer any expansion space.
            var parent = Context.Parent;
            while (parent != null)
            {
                switch (parent.State)
                {
                    case RowState { RawWidget: Row r }:
                    {
                        var finalMaxWidth = r.MainAxisSize == AxisSize.Max ? float.PositiveInfinity : maxWidth;
                        var finalMaxHeight = r.CrossAxisSize == AxisSize.Max ? float.PositiveInfinity : maxHeight;
                        return new WidgetSize(minWidth, minHeight, finalMaxWidth, finalMaxHeight);
                    }

                    // For a Column, Main Axis is Vertical, Cross Axis is Horizontal.
                    case ColumnState { RawWidget: Column c }:
                    {
                        var finalMaxHeight = c.MainAxisSize == AxisSize.Max ? float.PositiveInfinity : maxHeight;
                        var finalMaxWidth = c.CrossAxisSize == AxisSize.Max ? float.PositiveInfinity : maxWidth;
                        return new WidgetSize(minWidth, minHeight, finalMaxWidth, finalMaxHeight);
                    }
                    
                    
                    // case RowState {RawWidget: Row r}:
                    //     return (r.CrossAxisSize, r.MainAxisSize) switch
                    //     {
                    //         (AxisSize.Min, AxisSize.Min) => tightSize,
                    //         (AxisSize.Min, AxisSize.Max) => WidgetSize.FixedHeight(intrinsicWidth),
                    //         (AxisSize.Max, AxisSize.Min) => WidgetSize.FixedWidth(intrinsicHeight),
                    //         (AxisSize.Max, AxisSize.Max) => WidgetSize.Stretched,
                    //         _ => throw new NotSupportedException(
                    //             $"Unsupported combination of AxisSize: {r.MainAxisSize}, {r.CrossAxisSize}")
                    //     };
                    // case ColumnState {RawWidget: Column c}:
                    //     return (c.MainAxisSize, c.CrossAxisSize) switch
                    //     {
                    //         (AxisSize.Min, AxisSize.Min) => tightSize,
                    //         (AxisSize.Min, AxisSize.Max) => WidgetSize.FixedHeight(intrinsicWidth),
                    //         (AxisSize.Max, AxisSize.Min) => WidgetSize.FixedWidth(intrinsicHeight),
                    //         (AxisSize.Max, AxisSize.Max) => WidgetSize.Stretched,
                    //         _ => throw new NotSupportedException(
                    //             $"Unsupported combination of AxisSize: {c.MainAxisSize}, {c.CrossAxisSize}")
                    //     };
                    case ISingleChildLayoutState or ScrollGridFlowState:
                        // we found our container either way... lets stop the search.
                        return tightSize;
                    default:
                        parent = parent.Parent;
                        break;
                }
            }

            return tightSize;
        }

        
        public override WidgetViewReference View => WidgetViewReference.Resource("$$_LayoutHostView");
    }
}