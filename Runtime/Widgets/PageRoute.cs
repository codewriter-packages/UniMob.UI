namespace UniMob.UI.Widgets
{
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public class PageRoute : Route
    {
        [NotNull] private readonly WidgetBuilder _builder;

        internal BuildContext Context { get; set; }

        public PageRoute(
            [NotNull] string name,
            [NotNull] WidgetBuilder builder,
            RouteModalType modalType = RouteModalType.Popup
        ) : base(new ScreenSettings(name, modalType))
        {
            _builder = builder;
        }

        public override Widget Build(BuildContext context)
        {
            Context = context;
            return _builder(context);
        }

        public override bool HandleBack()
        {
            if (Context == null)
            {
                return base.HandleBack();
            }

            var navigator = Navigator.Of(Context, false, true);
            if (navigator == null)
            {
                return base.HandleBack();
            }

            navigator.Pop();
            return true;
        }
    }

    public delegate Widget AnimatedWidgetBuilder(BuildContext context, IAnimation<float> animation);

    public class AnimatedPageRoute : Route
    {
        private readonly AnimationController _tweenController;
        private readonly AnimatedWidgetBuilder _builder;

        internal BuildContext Context { get; set; }

        public AnimatedPageRoute(
            [NotNull] string name,
            [NotNull] AnimatedWidgetBuilder builder,
            float duration,
            float? reverseDuration = null,
            RouteModalType modalType = RouteModalType.Popup
        ) : base(new ScreenSettings(name, modalType))
        {
            _builder = builder;
            _tweenController = new AnimationController(duration, reverseDuration);
        }

        public override Widget Build(BuildContext context)
        {
            Context = context;
            return _builder(context, _tweenController.View);
        }

        protected override Task OnResume()
        {
            _tweenController.Forward();
            return base.OnResume();
        }

        protected override Task OnPause()
        {
            _tweenController.Reverse();
            return base.OnPause();
        }

        protected override async Task OnDestroy()
        {
            await _tweenController.Reverse();
            await base.OnDestroy();
        }

        public override bool HandleBack()
        {
            if (Context == null)
            {
                return base.HandleBack();
            }

            var navigator = Navigator.Of(Context, false, true);
            if (navigator == null)
            {
                return base.HandleBack();
            }

            navigator.Pop();
            return true;
        }
    }
}