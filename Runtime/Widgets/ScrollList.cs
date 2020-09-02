using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace UniMob.UI.Widgets
{
    public class ScrollList : MultiChildLayoutWidget
    {
        public ScrollList(
            [NotNull] List<Widget> children,
            [CanBeNull] Key key = null,
            CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Start,
            MainAxisAlignment mainAxisAlignment = MainAxisAlignment.Start
        ) : base(children, key)
        {
            CrossAxisAlignment = crossAxisAlignment;
            MainAxisAlignment = mainAxisAlignment;
        }

        public CrossAxisAlignment CrossAxisAlignment { get; }
        public MainAxisAlignment MainAxisAlignment { get; }

        public override State CreateState() => new ScrollListState();
    }

    internal class ScrollListState : MultiChildLayoutState<ScrollList>, IScrollListState
    {
        private readonly Atom<WidgetSize> _innerSize;

        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("UniMob.ScrollList");

        public ScrollListState()
        {
            _innerSize = Atom.Computed(CalculateInnerSize);
        }

        public override WidgetSize CalculateSize()
        {
            return WidgetSize.Stretched;
        }

        public WidgetSize InnerSize => _innerSize.Value;
        public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;
        public MainAxisAlignment MainAxisAlignment => Widget.MainAxisAlignment;

        private WidgetSize CalculateInnerSize()
        {
            float height = 0;
            float? width = 0;

            foreach (var child in Children)
            {
                var childSize = child.Size;

                if (childSize.IsHeightFixed)
                {
                    height += childSize.Height;
                }

                if (width.HasValue && childSize.IsWidthFixed)
                {
                    width = Math.Max(width.Value, childSize.Width);
                }
                else
                {
                    width = null;
                }
            }

            return new WidgetSize(width, height);
        }
    }
}