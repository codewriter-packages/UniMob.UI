using System;

namespace UniMob.UI.Widgets
{
    public class AnimatedCrossFade : StatefulWidget
    {
        public Widget FirstChild { get; set; } = new Empty();
        public Widget SecondChild { get; set; } = new Empty();
        public CrossFadeState CrossFadeState { get; set; } = CrossFadeState.ShowFirst;
        public float Duration { get; set; } = 0f;
        public float? ReverseDuration { get; set; } = null;
        public Alignment Alignment { get; set; } = Alignment.Center;
        public bool KeepMounted { get; set; } = false;

        public override State CreateState() => new AnimatedCrossFadeState();

        internal float GetReverseDuration() => ReverseDuration ?? Duration;
    }

    internal class AnimatedCrossFadeState : ViewState<AnimatedCrossFade>, IAnimatedCrossFadeState
    {
        private readonly Key _firstKey = Key.Of(CrossFadeState.ShowFirst);
        private readonly Key _secondKey = Key.Of(CrossFadeState.ShowSecond);

        private AnimationController _controller;
        private StateHolder _firstChild;
        private StateHolder _secondChild;

        private IAnimation<float> _firstAnimation;
        private IAnimation<float> _secondAnimation;

        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_AnimatedCrossFade");

        public override void InitState()
        {
            base.InitState();

            _firstChild = CreateChild(context =>
            {
                if (!Widget.KeepMounted && _controller.IsCompleted)
                {
                    return new Empty();
                }

                return new CompositeTransition
                {
                    Key = _firstKey,
                    Child = Widget.FirstChild,
                    Opacity = _firstAnimation,
                };
            });
            _secondChild = CreateChild(context =>
            {
                if (!Widget.KeepMounted && _controller.IsDismissed)
                {
                    return new Empty();
                }

                return new CompositeTransition
                {
                    Key = _secondKey,
                    Child = Widget.SecondChild,
                    Opacity = _secondAnimation,
                };
            });
            
            var completed = Widget.CrossFadeState == CrossFadeState.ShowSecond;
            _controller = new AnimationController(Widget.Duration, Widget.ReverseDuration, completed);

            _firstAnimation = _controller.Drive(new FloatTween(1, 0));
            _secondAnimation = _controller.Drive(new FloatTween(0, 1));
        }

        public override void DidUpdateWidget(AnimatedCrossFade oldWidget)
        {
            base.DidUpdateWidget(oldWidget);

            if (Math.Abs(oldWidget.Duration - Widget.Duration) > float.Epsilon)
            {
                _controller.Duration = Widget.Duration;
            }

            if (Math.Abs(oldWidget.GetReverseDuration() - Widget.GetReverseDuration()) > float.Epsilon)
            {
                _controller.ReverseDuration = Widget.GetReverseDuration();
            }

            if (oldWidget.CrossFadeState != Widget.CrossFadeState)
            {
                switch (Widget.CrossFadeState)
                {
                    case CrossFadeState.ShowFirst:
                        _controller.Reverse();
                        break;

                    case CrossFadeState.ShowSecond:
                        _controller.Forward();
                        break;
                }
            }
        }

        public override WidgetSize CalculateSize()
        {
            return WidgetSize.Lerp(FirstChild.Size, SecondChild.Size, _controller.Value);
        }

        public IState FirstChild => _firstChild.Value;
        public IState SecondChild => _secondChild.Value;
        public Alignment Alignment => Widget.Alignment;
    }
}