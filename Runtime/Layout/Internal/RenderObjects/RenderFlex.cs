using System.Collections.Generic;
using UniMob.UI.Layout.Views;
using UniMob.UI.Widgets;
using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    // The shared interface for Row/Column widgets.
    public interface IFlexWidget : IMultiChildLayoutWidget
    {
        CrossAxisAlignment CrossAxisAlignment { get; }
        MainAxisAlignment MainAxisAlignment { get; }
        AxisSize MainAxisSize { get; }
    }

    internal class RenderFlex : RenderObject, IMultiChildRenderObject
    {
        private readonly IFlexWidget _widget;
        private readonly IMultiChildLayoutState _state;
        private readonly Axis _axis;
        private float _unconstrainedMainAxisSize;
        private readonly List<LayoutData> _childrenLayout = new();
        public IReadOnlyList<LayoutData> ChildrenLayout => _childrenLayout;

        public RenderFlex(IFlexWidget widget, IMultiChildLayoutState state, Axis axis)
        {
            _widget = widget;
            _state = state;
            _axis = axis;
        }

        protected override Vector2 PerformSizing(LayoutConstraints constraints)
        {
            _childrenLayout.Clear();
            var mainAxisTotalSize = 0f;
            var crossAxisMaxSize = 0f;
            var totalFlexFactor = 0;

            var isHorizontal = _axis == Axis.Horizontal;
            var maxMainAxis = isHorizontal ? constraints.MaxWidth : constraints.MaxHeight;
            var maxCrossAxis = isHorizontal ? constraints.MaxHeight : constraints.MaxWidth;

            var nonFlexChildrenIndices = new List<int>();
            var flexChildrenData = new List<(int index, int flex)>();

            // --- First Pass: Identify flex vs. non-flex children ---
            for (var i = 0; i < _state.Children.Length; i++)
            {
                var childState = _state.Children[i];
                var childWidget = (childState as State)?.RawWidget;
                _childrenLayout.Add(new LayoutData()); // Add a placeholder

                var isFlexible = false;
                var flexFactor = 1;

                if (childWidget is Expanded expanded)
                {
                    isFlexible = true;
                    flexFactor = expanded.Flex;
                }

                // This is the key: if it's NOT a new layout widget, check if it's stretched.
                if (childWidget is not LayoutWidget)
                {
                    var legacySize = childState.Size;
                    if (isHorizontal ? float.IsInfinity(legacySize.MaxWidth) : float.IsInfinity(legacySize.MaxHeight))
                    {
                        isFlexible = true;
                    }
                }

                if (isFlexible)
                {
                    totalFlexFactor += flexFactor;
                    flexChildrenData.Add((i, flexFactor));
                }
                else
                {
                    nonFlexChildrenIndices.Add(i);
                }
            }

            // --- Second Pass: Measure non-flexible children ---
            var nonFlexConstraints = isHorizontal
                ? new LayoutConstraints(0, 0, float.PositiveInfinity, maxCrossAxis)
                : new LayoutConstraints(0, 0, maxCrossAxis, float.PositiveInfinity);

            foreach (var i in nonFlexChildrenIndices)
            {
                var childSize = LayoutChild(_state.Children[i], nonFlexConstraints);
                _childrenLayout[i] = new LayoutData {Size = childSize};
                mainAxisTotalSize += isHorizontal ? childSize.x : childSize.y;
                crossAxisMaxSize = Mathf.Max(crossAxisMaxSize, isHorizontal ? childSize.y : childSize.x);
            }

            // --- Third Pass: Layout flexible (Expanded or Stretched) children ---
            var freeSpace = maxMainAxis - mainAxisTotalSize;
            if (freeSpace < 0) freeSpace = 0;

            if (totalFlexFactor > 0)
            {
                foreach (var (i, flex) in flexChildrenData)
                {
                    var flexSpace = freeSpace * (flex / (float) totalFlexFactor);
                    var flexConstraints = isHorizontal
                        ? LayoutConstraints.Tight(flexSpace, maxCrossAxis)
                        : LayoutConstraints.Tight(maxCrossAxis, flexSpace);

                    var stateToLayout = _state.Children[i];
                    if ((stateToLayout as State)?.RawWidget is Expanded)
                    {
                        stateToLayout = ((ExpandedState) stateToLayout).Child;
                    }

                    var childSize = LayoutChild(stateToLayout, flexConstraints);
                    _childrenLayout[i] = new LayoutData {Size = childSize};
                    crossAxisMaxSize = Mathf.Max(crossAxisMaxSize, isHorizontal ? childSize.y : childSize.x);
                }
            }

            _unconstrainedMainAxisSize = mainAxisTotalSize + (totalFlexFactor > 0 ? freeSpace : 0);

            if (_widget.CrossAxisAlignment == CrossAxisAlignment.Stretch)
            {
                crossAxisMaxSize = maxCrossAxis;
            }

            var finalMainAxisSize = _widget.MainAxisSize == AxisSize.Max ? maxMainAxis : _unconstrainedMainAxisSize;

            var finalSize = isHorizontal
                ? new Vector2(finalMainAxisSize, crossAxisMaxSize)
                : new Vector2(crossAxisMaxSize, finalMainAxisSize);

            return new Vector2(
                Mathf.Clamp(finalSize.x, constraints.MinWidth, constraints.MaxWidth),
                Mathf.Clamp(finalSize.y, constraints.MinHeight, constraints.MaxHeight)
            );
        }

        protected override void PerformPositioning()
        {
            var mainAxisSize = _axis == Axis.Horizontal ? this.Size.x : this.Size.y;
            var freeSpace = (_axis == Axis.Horizontal ? this.Size.x : this.Size.y) - _unconstrainedMainAxisSize;
            float mainAxisPos = 0;
            float spacing = 0;
            var childCount = _childrenLayout.Count;
            if (freeSpace > 0 && !float.IsInfinity(mainAxisSize))
            {
                // --- MAIN AXIS ALIGNMENT ---
                switch (_widget.MainAxisAlignment)
                {
                    case MainAxisAlignment.Start:
                        mainAxisPos = 0;
                        break;
                    case MainAxisAlignment.Center:
                        mainAxisPos = freeSpace / 2f;
                        break;
                    case MainAxisAlignment.End:
                        mainAxisPos = freeSpace;
                        break;
                    case MainAxisAlignment.SpaceAround:
                        spacing = childCount > 0 ? freeSpace / childCount : 0;
                        mainAxisPos = spacing / 2f;
                        break;
                    case MainAxisAlignment.SpaceBetween:
                        spacing = childCount > 1 ? freeSpace / (childCount - 1) : 0;
                        break;
                    case MainAxisAlignment.SpaceEvenly:
                        spacing = childCount > 0 ? freeSpace / (childCount + 1) : 0;
                        mainAxisPos = spacing;
                        break;
                }
            }

            for (var i = 0; i < _childrenLayout.Count; i++)
            {
                var layout = _childrenLayout[i];

                // --- CROSS AXIS ALIGNMENT ---
                var crossAxisSize = _axis == Axis.Horizontal ? this.Size.y : this.Size.x;
                var childCrossAxisSize = _axis == Axis.Horizontal ? layout.Size.y : layout.Size.x;

                // Handle Stretch for this specific child
                if (_widget.CrossAxisAlignment == CrossAxisAlignment.Stretch)
                {
                    var newSize = _axis == Axis.Horizontal
                        ? new Vector2(layout.Size.x, crossAxisSize)
                        : new Vector2(crossAxisSize, layout.Size.y);
                    var newLayout = _childrenLayout[i];
                    newLayout.Size = newSize;
                    _childrenLayout[i] = newLayout;
                    childCrossAxisSize = crossAxisSize;
                }

                var crossAxisPos = _widget.CrossAxisAlignment switch
                {
                    CrossAxisAlignment.Center => (crossAxisSize - childCrossAxisSize) / 2f,
                    CrossAxisAlignment.End => crossAxisSize - childCrossAxisSize,
                    _ => 0, // Start and Stretch align to 0
                };
                var newLayoutData = _childrenLayout[i];
                newLayoutData.CornerPosition = _axis == Axis.Horizontal
                    ? new Vector2(mainAxisPos, crossAxisPos)
                    : new Vector2(crossAxisPos, mainAxisPos);
                _childrenLayout[i] = newLayoutData;
                mainAxisPos += (_axis == Axis.Horizontal ? layout.Size.x : layout.Size.y) + spacing;
            }
        }

        // --- GENERIC INTRINSIC SIZING ---
        public override float GetIntrinsicHeight(float width)
        {
            if (_axis == Axis.Vertical) // Sum of heights (for a Column)
            {
                float totalHeight = 0;
                foreach (var child in _state.Children)
                {
                    totalHeight += GetChildIntrinsicHeight(child, width);
                }

                return totalHeight;
            }
            else // Max of heights (for a Row)
            {
                float maxHeight = 0;
                // Cannot know width for each child, so we pass infinite. This is a limitation.
                foreach (var child in _state.Children)
                {
                    maxHeight = Mathf.Max(maxHeight, GetChildIntrinsicHeight(child, float.PositiveInfinity));
                }

                return maxHeight;
            }
        }

        public override float GetIntrinsicWidth(float height)
        {
            if (_axis == Axis.Horizontal) // Sum of widths (for a Row)
            {
                float totalWidth = 0;
                foreach (var child in _state.Children)
                {
                    totalWidth += GetChildIntrinsicWidth(child, height);
                }

                return totalWidth;
            }
            else // Max of widths (for a Column)
            {
                float maxWidth = 0;
                foreach (var child in _state.Children)
                {
                    maxWidth = Mathf.Max(maxWidth, GetChildIntrinsicWidth(child, float.PositiveInfinity));
                }

                return maxWidth;
            }
        }

        private float GetChildIntrinsicWidth(IState child, float height)
        {
            if (child is ILayoutState cls) return cls.RenderObject.GetIntrinsicWidth(height);
            return child.Size.GetSizeUnbounded().x;
        }

        private float GetChildIntrinsicHeight(IState child, float width)
        {
            if (child is ILayoutState cls) return cls.RenderObject.GetIntrinsicHeight(width);
            return child.Size.GetSize(new Vector2(width, float.PositiveInfinity)).y;
        }
    }
}