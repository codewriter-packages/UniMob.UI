using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    public abstract class PageRoute : Route
    {
        private readonly AnimationController _animation;
        private readonly AnimationController _secondaryAnimation;

        private bool _destroying;
        private bool _paused;

        protected PageRoute(
            RouteSettings routeSettings,
            float transitionDuration,
            float reverseTransitionDuration
        ) : base(routeSettings)
        {
            _animation = new AnimationController(transitionDuration, reverseTransitionDuration);
            _secondaryAnimation = new AnimationController(transitionDuration, reverseTransitionDuration, true);
        }

        public sealed override Widget Build(BuildContext context)
        {
            var child = BuildPage(context, _animation, _secondaryAnimation);
            return BuildTransitions(context, _animation, _secondaryAnimation, child);
        }

        protected abstract Widget BuildPage(BuildContext context, AnimationController animation,
            AnimationController secondaryAnimation);

        protected abstract Widget BuildTransitions(BuildContext context, AnimationController animation,
            AnimationController secondaryAnimation, Widget child);

        public override bool HandleBack()
        {
            if (NavigatorState == null)
            {
                return base.HandleBack();
            }

            NavigatorState.Pop();
            return true;
        }

        public override Task ApplyScreenEvent(ScreenEvent screenEvent)
        {
            _destroying = screenEvent == ScreenEvent.Destroy;

            return base.ApplyScreenEvent(screenEvent);
        }

        protected override Task OnResume()
        {
            if (_paused)
            {
                _secondaryAnimation.Forward();
                _animation.Complete();
            }
            else
            {
                _animation.Forward();
                _secondaryAnimation.Complete();
            }

            _paused = false;

            return base.OnResume();
        }

        protected override Task OnPause()
        {
            _paused = true;

            if (_destroying)
            {
                _animation.Reverse();
                _secondaryAnimation.Complete();
            }
            else
            {
                _animation.Complete();
                _secondaryAnimation.Reverse();
            }

            return base.OnPause();
        }

        protected override async Task OnDestroy()
        {
            await Atom.When(() => !_animation.IsAnimating);
            await base.OnDestroy();
        }
    }

    public delegate Widget PageBuilder(BuildContext context, AnimationController animation,
        AnimationController secondaryAnimation);

    public delegate Widget PageTransitionsBuilder(BuildContext context, AnimationController animation,
        AnimationController secondaryAnimation, Widget child);

    public class PageRouteBuilder : PageRoute
    {
        private readonly PageBuilder _pageBuilder;
        private readonly PageTransitionsBuilder _transitionsBuilder;

        public PageRouteBuilder(
            RouteSettings settings,
            PageBuilder pageBuilder,
            PageTransitionsBuilder transitionsBuilder = null,
            float transitionDuration = 0f,
            float reverseTransitionDuration = 0f
        ) : base(settings, transitionDuration, reverseTransitionDuration)
        {
            _pageBuilder = pageBuilder;
            _transitionsBuilder = transitionsBuilder ?? DefaultTransitionsBuilder;
        }

        private static readonly PageTransitionsBuilder DefaultTransitionsBuilder =
            (context, animation, secondaryAnimation, child) => child;

        protected override Widget BuildPage(BuildContext context, AnimationController animation,
            AnimationController secondaryAnimation)
        {
            return _pageBuilder(context, animation, secondaryAnimation);
        }

        protected override Widget BuildTransitions(BuildContext context, AnimationController animation,
            AnimationController secondaryAnimation, Widget child)
        {
            return _transitionsBuilder(context, animation, secondaryAnimation, child);
        }
    }
}