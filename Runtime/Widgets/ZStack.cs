using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace UniMob.UI.Widgets
{
    public class ZStack : MultiChildLayoutWidget
    {
        public Alignment Alignment { get; }

        public ZStack(
            [NotNull] List<Widget> children,
            [CanBeNull] Key key = null,
            Alignment? alignment = null)
            : base(children, key)
        {
            Alignment = alignment ?? Alignment.Center;
        }

        public override State CreateState() => new ZStackState();
    }

    internal class ZStackState : MultiChildLayoutState<ZStack>, IZStackState
    {
        private readonly Atom<WidgetSize> _innerSize;

        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_ZStack");

        public ZStackState()
        {
            _innerSize = Atom.Computed(CalculateInnerSize);
        }

        public Alignment Alignment => Widget.Alignment;

        public override WidgetSize CalculateSize()
        {
            return _innerSize.Value;
        }

        private WidgetSize CalculateInnerSize()
        {
            float? height = 0;
            float? width = 0;

            foreach (var child in Children)
            {
                var childSize = child.Size;

                height = height.HasValue && childSize.IsHeightFixed
                    ? Math.Max(height.Value, childSize.Height)
                    : default(float?);

                width = width.HasValue && childSize.IsWidthFixed
                    ? Math.Max(width.Value, childSize.Width)
                    : default(float?);
            }

            return new WidgetSize(width, height);
        }
    }
}