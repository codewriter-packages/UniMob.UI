namespace UniMob.UI.Widgets
{
    using System;
    using UniMob.UI;

    public class ScrollGridFlow : MultiChildLayoutWidget
    {
        public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;
        public int MaxCrossAxisCount { get; set; } = int.MaxValue;
        public float MaxCrossAxisExtent { get; set; } = int.MaxValue;
        public bool UseMask { get; set; } = true;
        public Key Sticky { get; set; } = null;

        public override State CreateState() => new ScrollGridFlowState();
    }

    public class ScrollGridFlowState : MultiChildLayoutState<ScrollGridFlow>, IScrollGridFlowState
    {
        private ScrollGridFlowView _gridView;

        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("UniMob.ScrollGridFlow");

        [Atom] public WidgetSize InnerSize => CalculateInnerSize();
        public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;
        public int MaxCrossAxisCount => Widget.MaxCrossAxisCount;
        public float MaxCrossAxisExtent => Widget.MaxCrossAxisExtent;
        public bool UseMask => Widget.UseMask;
        public Key Sticky => Widget.Sticky;

        public override WidgetSize CalculateSize()
        {
            return WidgetSize.Stretched;
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

        public override void DidViewMount(IView view)
        {
            base.DidViewMount(view);

            _gridView = view as ScrollGridFlowView;
        }

        public override void DidViewUnmount(IView view)
        {
            base.DidViewUnmount(view);

            _gridView = null;
        }

        public void ScrollTo(Key key, float duration)
        {
            if (_gridView == null)
            {
                return;
            }

            _gridView.ScrollTo(key, duration);
        }
    }
}