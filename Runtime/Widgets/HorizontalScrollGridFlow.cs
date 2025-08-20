namespace UniMob.UI.Widgets
{
    using UnityEngine;

    public class HorizontalScrollGridFlow : MultiChildLayoutWidget
    {
        public static WidgetViewReference DefaultView =
            WidgetViewReference.Resource("UniMob.HorizontalScrollGridFlow");

        public MainAxisAlignment MainAxisAlignment { get; set; } = MainAxisAlignment.Start;
        public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;
        public RectPadding Padding { get; set; }
        public Vector2 Spacing { get; set; }
        public int MaxCrossAxisCount { get; set; } = int.MaxValue;
        public float MaxCrossAxisExtent { get; set; } = float.PositiveInfinity;
        public GridLayoutDelegate LayoutDelegate { get; set; }
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
        private readonly StateHolder _backgroundContent;

        private HorizontalScrollGridFlowView _gridView;

        [Atom] public override WidgetViewReference View => Widget.View;

        [Atom] public WidgetSize InnerSize => CalculateInnerSize();
        public MainAxisAlignment MainAxisAlignment => Widget.MainAxisAlignment;
        public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;
        public int MaxCrossAxisCount => Widget.MaxCrossAxisCount;
        public float MaxCrossAxisExtent => Widget.MaxCrossAxisExtent;
        public bool UseMask => Widget.UseMask;
        public Key Sticky => Widget.Sticky;
        public StickyModes StickyMode => Widget.StickyMode;
        public IState BackgroundContent => _backgroundContent.Value;

        public GridLayoutSettings LayoutSettings => new GridLayoutSettings
        {
            mainAxis = 1,
            children = Children,
            gridPadding = Widget.Padding,
            spacing = Widget.Spacing,
            maxLineWidth = Widget.MaxCrossAxisExtent,
            maxLineChildNum = Widget.MaxCrossAxisCount,
        };

        public GridLayoutDelegate LayoutDelegate => Widget.LayoutDelegate ?? GridLayoutUtility.DefaultLayoutDelegate;

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
            return GridLayoutUtility.CalculateSize(LayoutSettings, LayoutDelegate);
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

        public bool ScrollTo(Key key, float duration, float offset = 0, Easing easing = null)
        {
            if (_gridView == null)
            {
                return false;
            }

            return _gridView.ScrollTo(key, duration, offset, easing ?? Ease.InOutCirc);
        }
    }
}