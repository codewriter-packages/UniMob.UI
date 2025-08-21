using System;
using System.Collections.Generic;
using UniMob.UI.Widgets;
using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    // A simple struct to pair a child's original index with its final layout data.
    internal struct IndexedLayoutData
    {
        public int ChildIndex;
        public LayoutData Layout;
    }

    internal interface ISliverState : ILayoutState
    {
        IState[] AllChildren { get; }
        Axis Axis { get; }
        Vector2 ViewportSize { get; }
        float ScrollOffset { get; }
        float VirtualizationCacheExtent { get; }

        internal void SetVisibleChildren(List<IndexedLayoutData> visibleChildren);
    }

    internal class RenderSliverList : RenderObject, IMultiChildRenderObject
    {
        // Caches the measured sizes of all children to avoid re-calculating every frame.
        private readonly List<Vector2> _allChildrenSizes = new();
        private readonly ISliverState _state;
        private readonly List<LayoutData> _visibleChildrenLayout = new();

        public RenderSliverList(ISliverState state)
        {
            _state = state;
        }

        public IReadOnlyList<LayoutData> ChildrenLayout => _visibleChildrenLayout;

        /// <summary>
        ///     SIZING PASS: Measures every child to determine the total scrollable content size.
        /// </summary>
        protected override Vector2 PerformSizing(LayoutConstraints constraints)
        {
            _allChildrenSizes.Clear();
            var isHorizontal = _state.Axis == Axis.Horizontal;

            if (isHorizontal && !constraints.HasBoundedWidth)
                throw new InvalidOperationException(
                    "A horizontal ScrollingList must have a bounded width." +
                    "This usually means it is being placed inside another horizontal scrollable or a Row without an Expanded widget."
                );

            if (!isHorizontal && !constraints.HasBoundedHeight)
                throw new InvalidOperationException(
                    "A vertical ScrollingList must have a bounded height." +
                    "This usually means it is being placed inside another vertical scrollable or a Column without an Expanded widget."
                );

            // Give children unconstrained space along the main scrolling axis
            // but constrain them to the viewport's size on the cross axis.
            LayoutConstraints childConstraints;
            if (isHorizontal)
                // For a horizontal list, height is tight, width is loose.
                childConstraints = new LayoutConstraints(0, constraints.MaxHeight, float.PositiveInfinity,
                    constraints.MaxHeight);
            else
                // For a vertical list, width is tight, height is loose.
                childConstraints = new LayoutConstraints(constraints.MaxWidth, 0, constraints.MaxWidth,
                    float.PositiveInfinity);
            

            foreach (var child in _state.AllChildren)
            {
                var childSize = LayoutChild(child, childConstraints);
                _allChildrenSizes.Add(childSize);
            }

            // The RenderObject's own size is simply the size of the viewport,
            // as dictated by the parent's constraints.
            return new Vector2(constraints.MaxWidth, constraints.MaxHeight);
        }

        /// <summary>
        ///     POSITIONING PASS: Calculates positions for ONLY the visible children.
        /// </summary>
        protected override void PerformPositioning()
        {
            var visibleChildrenData = new List<IndexedLayoutData>();
            var cacheExtent = _state.VirtualizationCacheExtent; // The buffer for smooth scrolling.

            var isHorizontal = _state.Axis == Axis.Horizontal;
            var scrollOffset = _state.ScrollOffset;
            var viewportMainAxisSize = isHorizontal ? _state.ViewportSize.x : _state.ViewportSize.y;

            var viewportStart = scrollOffset - cacheExtent;
            var viewportEnd = scrollOffset + viewportMainAxisSize + cacheExtent;

            float mainAxisPos = 0;
            for (var i = 0; i < _allChildrenSizes.Count; i++)
            {
                var childSize = _allChildrenSizes[i];
                var childMainAxisSize = isHorizontal ? childSize.x : childSize.y;

                var childStart = mainAxisPos;
                var childEnd = childStart + childMainAxisSize;

                // The core culling logic: if the child intersects the viewport (plus buffer), it's visible.
                if (childEnd > viewportStart && childStart < viewportEnd)
                {
                    // Calculate position relative to the viewport's top-left corner.
                    var cornerPosition = isHorizontal
                        ? new Vector2(mainAxisPos, 0)
                        : new Vector2(0, mainAxisPos);

                    visibleChildrenData.Add(new IndexedLayoutData
                    {
                        ChildIndex = i,
                        Layout = new LayoutData
                        {
                            Size = childSize,
                            CornerPosition = cornerPosition
                        }
                    });
                }

                mainAxisPos += childMainAxisSize;
            }

            _visibleChildrenLayout.Clear();
            foreach (var visibleChild in visibleChildrenData) _visibleChildrenLayout.Add(visibleChild.Layout);

            // Push the results back to the state.
            _state.SetVisibleChildren(visibleChildrenData);
        }

        // Intrinsic sizing is used to determine the total size of the scrollable content.
        public override float GetIntrinsicHeight(float width)
        {
            if (_state.Axis == Axis.Horizontal) return 0; // Not meaningful for a horizontal list

            float totalHeight = 0;
            foreach (var child in _state.AllChildren) totalHeight += GetChildIntrinsicHeight(child, width);

            return totalHeight;
        }

        public override float GetIntrinsicWidth(float height)
        {
            if (_state.Axis == Axis.Vertical) return 0; // Not meaningful for a vertical list

            float totalWidth = 0;
            foreach (var child in _state.AllChildren) totalWidth += GetChildIntrinsicWidth(child, height);

            return totalWidth;
        }


        private static float GetChildIntrinsicWidth(IState child, float height)
        {
            if (child.AsLayoutState(out var cls)) return cls.RenderObject.GetIntrinsicWidth(height);
            return child.Size.GetSizeUnbounded().x;
        }

        private static float GetChildIntrinsicHeight(IState child, float width)
        {
            if (child.AsLayoutState(out var cls)) return cls.RenderObject.GetIntrinsicHeight(width);
            return child.Size.GetSize(new Vector2(width, float.PositiveInfinity)).y;
        }

        public float CalculateNormalizedOffset(int index, ScrollToPosition position)
        {
            var isHorizontal = _state.Axis == Axis.Horizontal;
            var viewportSize = isHorizontal ? _state.ViewportSize.x : _state.ViewportSize.y;

            var totalContentSize = isHorizontal
                ? GetIntrinsicWidth(_state.ViewportSize.y)
                : GetIntrinsicHeight(_state.ViewportSize.x);

            var totalScrollableDist = totalContentSize - viewportSize;

            // If there's nothing to scroll (content fits in the viewport), the offset is always 0.
            if (totalScrollableDist <= 0 || index < 0 || index >= _allChildrenSizes.Count)
                return 0;

            // Get the pixel offset of the target child.
            float childOffset = 0;
            for (var i = 0; i < index; i++)
                childOffset += isHorizontal ? _allChildrenSizes[i].x : _allChildrenSizes[i].y;

            var childSize = isHorizontal ? _allChildrenSizes[index].x : _allChildrenSizes[index].y;

            switch (position)
            {
                case ScrollToPosition.Start:
                    // childOffset remains unchanged, already at the start position.
                    break;
                case ScrollToPosition.Center:
                    childOffset -= (viewportSize - childSize) / 2;
                    break;
                case ScrollToPosition.End:
                    childOffset -= viewportSize - childSize;
                    break;
            }

            // Convert the pixel offset to a normalized value based on the SCROLLABLE distance.
            return childOffset / totalScrollableDist;
        }
    }
}