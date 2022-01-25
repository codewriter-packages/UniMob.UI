using System;
using UnityEngine;

namespace UniMob.UI
{
    public struct WidgetSize : IEquatable<WidgetSize>
    {
        public float MinWidth { get; }
        public float MinHeight { get; }
        public float MaxWidth { get; }
        public float MaxHeight { get; }

        public static WidgetSize Zero { get; } = new WidgetSize(0, 0, 0, 0);

        public static WidgetSize Stretched { get; } =
            new WidgetSize(0, 0, float.PositiveInfinity, float.PositiveInfinity);

        public WidgetSize(float minWidth, float minHeight, float maxWidth, float maxHeight)
        {
            MinWidth = Mathf.Max(0, minWidth);
            MinHeight = Mathf.Max(0, minHeight);
            MaxWidth = Mathf.Max(minWidth, maxWidth);
            MaxHeight = Mathf.Max(minHeight, maxHeight);
        }

        public void Deconstruct(out float minWidth, out float minHeight, out float maxWidth, out float maxHeight)
        {
            minWidth = MinWidth;
            minHeight = MinHeight;
            maxWidth = MaxWidth;
            maxHeight = MaxHeight;
        }

        public static WidgetSize Fixed(float w, float h)
        {
            return new WidgetSize(w, h, w, h);
        }

        public static WidgetSize FixedWidth(float w)
        {
            return new WidgetSize(w, 0, w, float.PositiveInfinity);
        }

        public static WidgetSize FixedHeight(float h)
        {
            return new WidgetSize(0, h, float.PositiveInfinity, h);
        }

        internal Vector2 GetSizeUnbounded()
        {
            return GetSize(new Vector2(float.PositiveInfinity, float.PositiveInfinity));
        }

        internal Vector2 GetSize(Vector2 bounds)
        {
            var w = (bounds.x < MinWidth) ? MinWidth : Mathf.Min(bounds.x, MaxWidth);
            var h = (bounds.y < MinHeight) ? MinHeight : Mathf.Min(bounds.y, MaxHeight);
            return new Vector2(w, h);
        }

        internal static WidgetSize Lerp(WidgetSize a, WidgetSize b, float t)
        {
            if (Mathf.Approximately(t, 0))
            {
                return a;
            }

            if (Mathf.Approximately(t, 1))
            {
                return b;
            }

            var minWidth = Mathf.Lerp(a.MinWidth, b.MinWidth, t);
            var minHeight = Mathf.Lerp(a.MinHeight, b.MinHeight, t);
            var maxWidth = float.PositiveInfinity;
            var maxHeight = float.PositiveInfinity;

            if (!float.IsInfinity(a.MaxWidth) || !float.IsInfinity(b.MaxWidth))
            {
                var w1 = float.IsInfinity(a.MaxWidth) ? b.MaxWidth : a.MaxWidth;
                var w2 = float.IsInfinity(b.MaxWidth) ? a.MaxWidth : b.MaxWidth;
                maxWidth = Mathf.Lerp(w1, w2, t);
            }

            if (!float.IsInfinity(a.MaxHeight) || !float.IsInfinity(b.MaxHeight))
            {
                var h1 = float.IsInfinity(a.MaxHeight) ? b.MaxHeight : a.MaxHeight;
                var h2 = float.IsInfinity(b.MaxHeight) ? a.MaxHeight : b.MaxHeight;
                maxHeight = Mathf.Lerp(h1, h2, t);
            }

            return new WidgetSize(minWidth, minHeight, maxWidth, maxHeight);
        }

        internal static WidgetSize StackX(WidgetSize a, WidgetSize b)
        {
            return new WidgetSize(
                minWidth: a.MinWidth + b.MinWidth,
                minHeight: Mathf.Max(a.MinHeight, b.MinHeight),
                maxWidth: a.MaxWidth + b.MaxWidth,
                maxHeight: Mathf.Min(a.MaxHeight, b.MaxHeight)
            );
        }

        internal static WidgetSize StackY(WidgetSize a, WidgetSize b)
        {
            return new WidgetSize(
                minWidth: Mathf.Max(a.MinWidth, b.MinWidth),
                minHeight: a.MinHeight + b.MinHeight,
                maxWidth: Mathf.Min(a.MaxWidth, b.MaxWidth),
                maxHeight: a.MaxHeight + b.MaxHeight
            );
        }

        internal static WidgetSize StackZ(WidgetSize a, WidgetSize b)
        {
            return new WidgetSize(
                minWidth: Mathf.Max(a.MinWidth, b.MinWidth),
                minHeight: Mathf.Max(a.MinHeight, b.MinHeight),
                maxWidth: Mathf.Min(a.MaxWidth, b.MaxWidth),
                maxHeight: Mathf.Min(a.MaxHeight, b.MaxHeight)
            );
        }

        public bool Equals(WidgetSize other)
        {
            return Mathf.Approximately(MinWidth, other.MinWidth) &&
                   Mathf.Approximately(MinHeight, other.MinHeight) &&
                   Mathf.Approximately(MaxWidth, other.MaxWidth) &&
                   Mathf.Approximately(MaxHeight, other.MaxHeight);
        }

        public override bool Equals(object obj)
        {
            return obj is WidgetSize other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = MinWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ MinHeight.GetHashCode();
                hashCode = (hashCode * 397) ^ MaxWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ MaxHeight.GetHashCode();
                return hashCode;
            }
        }
    }
}