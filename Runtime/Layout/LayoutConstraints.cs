using System;
using UnityEngine;

namespace UniMob.UI.Layout
{
    public readonly struct LayoutConstraints : IEquatable<LayoutConstraints>
    {
        public float MinWidth { get; }
        public float MinHeight { get; }
        public float MaxWidth { get; }
        public float MaxHeight { get; }

        public LayoutConstraints(float minWidth, float minHeight, float maxWidth, float maxHeight)
        {
            MinWidth = minWidth;
            MinHeight = minHeight;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
        }

        /// <summary>
        ///     Creates a set of tight constraints, forcing an exact size.
        /// </summary>
        public static LayoutConstraints Tight(float width, float height)
        {
            return new LayoutConstraints(width, height, width, height);
        }

        /// <summary>
        ///     Creates a set of loose constraints, allowing any size up to the specified maximum.
        /// </summary>
        public static LayoutConstraints Loose(float width, float height)
        {
            return new LayoutConstraints(0, 0, width, height);
        }

        
        /// <summary>
        ///     Creates a set of unbounded constraints, allowing any size.
        /// </summary>
        public static LayoutConstraints Unbounded()
        {
            return new LayoutConstraints(0, 0, float.PositiveInfinity, float.PositiveInfinity);
        }


        public bool HasTightWidth => MinWidth >= MaxWidth;
        public bool HasTightHeight => MinHeight >= MaxHeight;
        public bool IsTight => HasTightWidth && HasTightHeight;
        public bool HasBoundedWidth => float.IsFinite(MaxWidth);
        public bool HasBoundedHeight => float.IsFinite(MaxHeight);

        /// <summary>
        ///     Creates a new set of constraints with the minimums removed.
        /// </summary>
        public LayoutConstraints Loosen()
        {
            return new LayoutConstraints(0, 0, MaxWidth, MaxHeight);
        }

        /// <summary>
        ///     Creates a new set of constraints by enforcing the bounds of another.
        ///     The final constraints will be within the bounds of both.
        /// </summary>
        /// <remarks>
        ///     This operation is <i>not</i> commutative, i.e. <c>a.Enforce(b)</c>
        ///     is not the same as <c>b.Enforce(a)</c>.
        /// </remarks>
        public LayoutConstraints Enforce(LayoutConstraints other)
        {
            return new LayoutConstraints(
                Mathf.Clamp(MinWidth, other.MinWidth, other.MaxWidth),
                Mathf.Clamp(MinHeight, other.MinHeight, other.MaxHeight),
                Mathf.Clamp(MaxWidth, other.MinWidth, other.MaxWidth),
                Mathf.Clamp(MaxHeight, other.MinHeight, other.MaxHeight)
            );
        }

        /// <summary>
        ///     Returns a new set of constraints that is deflated by the given padding.
        ///     The minimums will never go below zero.
        /// </summary>
        public LayoutConstraints Deflate(RectPadding padding)
        {
            var horizontal = padding.Horizontal;
            var vertical = padding.Vertical;
            var deflatedMinWidth = Mathf.Max(0, MinWidth - horizontal);
            var deflatedMinHeight = Mathf.Max(0, MinHeight - vertical);

            return new LayoutConstraints(
                deflatedMinWidth,
                deflatedMinHeight,
                Mathf.Max(deflatedMinWidth, MaxWidth - horizontal),
                Mathf.Max(deflatedMinHeight, MaxHeight - vertical)
            );
        }
        
        /// <summary>
        ///     Creates a new set of constraints by tightening the minimums.
        /// </summary>
        public LayoutConstraints Tighten(float? width = null, float? height = null)
        {
            return new LayoutConstraints(
                width.HasValue ? Mathf.Clamp(width.Value, MinWidth, MaxWidth) : MinWidth,
                height.HasValue ? Mathf.Clamp(height.Value, MinHeight, MaxHeight) : MinHeight,
                MaxWidth,
                MaxHeight
            );
        }

        /// <summary>
        ///     Returns a size that respects these constraints.
        /// </summary>
        public Vector2 Constrain(Vector2 size)
        {
            return new Vector2(
                Mathf.Clamp(size.x, MinWidth, MaxWidth),
                Mathf.Clamp(size.y, MinHeight, MaxHeight)
            );
        }

        public override string ToString()
        {
            return $"Constraints(w:[{MinWidth}-{MaxWidth}], h:[{MinHeight}-{MaxHeight}])";
        }

        public bool Equals(LayoutConstraints other)
        {
            return MinWidth.Equals(other.MinWidth) &&
                   MinHeight.Equals(other.MinHeight) &&
                   MaxWidth.Equals(other.MaxWidth) &&
                   MaxHeight.Equals(other.MaxHeight);
        }

        public override bool Equals(object obj)
        {
            return obj is LayoutConstraints other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MinWidth, MinHeight, MaxWidth, MaxHeight);
        }

        
    }
}