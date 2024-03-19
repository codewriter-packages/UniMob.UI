namespace UniMob.UI.Widgets
{
    using System;
    using UnityEngine;

    public class HorizontalScrollGridFlow : MultiChildLayoutWidget
    {
        public static WidgetViewReference DefaultView =
            WidgetViewReference.Resource("UniMob.HorizontalScrollGridFlow");

        public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;
        public int MaxCrossAxisCount { get; set; } = int.MaxValue;
        public float MaxCrossAxisExtent { get; set; } = float.PositiveInfinity;
        public bool UseMask { get; set; } = true;
        public Key Sticky { get; set; } = null;
        public StickyModes StickyMode { get; set; } = StickyModes.Top;
        public Widget BackgroundContent { get; set; } = null;
        public WidgetViewReference View { get; set; } = DefaultView;

        public ScrollController ScrollController { get; set; }

        public override State CreateState() => new HorizontalScrollGridFlowState();
    }

    public class HorizontalScrollGridFlowState : MultiChildLayoutState<HorizontalScrollGridFlow>,
        IHorizontalScrollGridFlowState
    {
        private static readonly ScrollEasing CircEaseInOutEasing = (t, d) =>
            (t /= d / 2) < 1 ? -0.5f * (Mathf.Sqrt(1 - t * t) - 1) : 0.5f * (Mathf.Sqrt(1 - (t -= 2) * t) + 1);

        private readonly StateHolder _backgroundContent;

        private HorizontalScrollGridFlowView _gridView;

        [Atom] public override WidgetViewReference View => Widget.View;

        [Atom] public WidgetSize InnerSize => CalculateInnerSize();
        public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;
        public int MaxCrossAxisCount => Widget.MaxCrossAxisCount;
        public float MaxCrossAxisExtent => Widget.MaxCrossAxisExtent;
        public bool UseMask => Widget.UseMask;
        public Key Sticky => Widget.Sticky;
        public StickyModes StickyMode => Widget.StickyMode;
        public IState BackgroundContent => _backgroundContent.Value;

        [Atom] public ScrollController ScrollController { get; private set; }

        public HorizontalScrollGridFlowState()
        {
            _backgroundContent = CreateChild(_ => Widget.BackgroundContent ?? new Empty());
        }

        public override void InitState()
        {
            base.InitState();

            ScrollController = Widget.ScrollController ?? new ScrollController(StateLifetime);
        }

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

            var maxLineHeight = Widget.MaxCrossAxisExtent;
            var maxLineChildNum = Widget.MaxCrossAxisCount;

            foreach (var child in Children)
            {
                var childSize = child.Size;

                if (float.IsInfinity(childSize.MaxWidth))
                {
                    width = float.PositiveInfinity;
                    continue;
                }

                if (lineChildNum + 1 <= maxLineChildNum &&
                    lineHeight + childSize.MaxHeight <= maxLineHeight)
                {
                    lineChildNum++;
                    lineHeight += childSize.MaxHeight;
                    lineWidth = Math.Max(lineWidth, childSize.MaxWidth);
                }
                else
                {
                    height = Math.Max(height, lineHeight);
                    width += lineWidth;

                    lineChildNum = 1;
                    lineHeight = childSize.MaxHeight;
                    lineWidth = childSize.MaxWidth;
                }
            }

            height = Math.Max(height, lineHeight);
            width += lineWidth;

            height = Math.Min(height, MaxCrossAxisExtent);

            return WidgetSize.Fixed(width, height);
        }

        public override void DidViewMount(IView view)
        {
            base.DidViewMount(view);

            _gridView = view as HorizontalScrollGridFlowView;
        }

        public override void DidViewUnmount(IView view)
        {
            base.DidViewUnmount(view);

            _gridView = null;
        }

        public override void DidUpdateWidget(HorizontalScrollGridFlow oldWidget)
        {
            base.DidUpdateWidget(oldWidget);

            if (Widget.ScrollController != null && Widget.ScrollController != ScrollController)
            {
                ScrollController = Widget.ScrollController;
            }
        }

        public bool ScrollTo(Key key, float duration, float offset = 0, ScrollEasing easing = null)
        {
            if (_gridView == null)
            {
                return false;
            }

            return _gridView.ScrollTo(key, duration, offset, easing ?? CircEaseInOutEasing);
        }
    }
}