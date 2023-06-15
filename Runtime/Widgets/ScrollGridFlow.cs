namespace UniMob.UI.Widgets
{
    using System;
    using UnityEngine;

    public class ScrollGridFlow : MultiChildLayoutWidget
    {
        public static WidgetViewReference DefaultView =
            WidgetViewReference.Resource("UniMob.ScrollGridFlow");

        public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;

        

        public int MaxCrossAxisCount { get; set; } = int.MaxValue;
        public float MaxCrossAxisExtent { get; set; } = float.PositiveInfinity;
        public bool UseMask { get; set; } = true;
        public Key Sticky { get; set; } = null;
        public StickyModes StickyMode { get; set; } = StickyModes.Top;
        public Widget BackgroundContent { get; set; } = null;
        public WidgetViewReference View { get; set; } = DefaultView;

        public ScrollController ScrollController { get; set; }

        public MovementType MovementType { get; set; } = MovementType.Elastic;
        public override State CreateState() => new ScrollGridFlowState();
    }

    public class ScrollGridFlowState : MultiChildLayoutState<ScrollGridFlow>, IScrollGridFlowState
    {
        private static readonly ScrollEasing CircEaseInOutEasing = (t, d) =>
            (t /= d / 2) < 1 ? -0.5f * (Mathf.Sqrt(1 - t * t) - 1) : 0.5f * (Mathf.Sqrt(1 - (t -= 2) * t) + 1);

        private readonly StateHolder _backgroundContent;

        private ScrollGridFlowView _gridView;

        [Atom] public override WidgetViewReference View => Widget.View;

        [Atom] public WidgetSize InnerSize => CalculateInnerSize();
        public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;

        public MovementType MovementType => Widget.MovementType;
        public int MaxCrossAxisCount => Widget.MaxCrossAxisCount;
        public float MaxCrossAxisExtent => Widget.MaxCrossAxisExtent;
        public bool UseMask => Widget.UseMask;
        public Key Sticky => Widget.Sticky;
        public StickyModes StickyMode => Widget.StickyMode;
        public IState BackgroundContent => _backgroundContent.Value;

        [Atom] public ScrollController ScrollController { get; private set; }

        public ScrollGridFlowState()
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

            var maxLineWidth = Widget.MaxCrossAxisExtent;
            var maxLineChildNum = Widget.MaxCrossAxisCount;

            foreach (var child in Children)
            {
                var childSize = child.Size;

                if (float.IsInfinity(childSize.MaxHeight))
                {
                    height = float.PositiveInfinity;
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

        public override void DidUpdateWidget(ScrollGridFlow oldWidget)
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

    [Flags]
    public enum StickyModes
    {
        Top = 1 << 0,
        Bottom = 1 << 1,
        TopAndBottom = Top | Bottom,
    }

    public delegate float ScrollEasing(float t, float duration);
}