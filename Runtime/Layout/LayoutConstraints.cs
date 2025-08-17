using System;

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
        /// Creates a set of tight constraints, forcing an exact size.
        /// </summary>
        public static LayoutConstraints Tight(float width, float height)
        {
            return new LayoutConstraints(width, height, width, height);
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