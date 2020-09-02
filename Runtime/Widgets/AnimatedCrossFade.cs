using System;
using JetBrains.Annotations;

namespace UniMob.UI.Widgets
{
    public class AnimatedCrossFade : StatefulWidget
    {
        public AnimatedCrossFade(
            Widget firstChild,
            Widget secondChild,
            CrossFadeState crossFadeState,
            float duration,
            float? reverseDuration = null,
            Alignment? alignment = null,
            [CanBeNull] Key key = null)
            : base(key)
        {
            FirstChild = firstChild;
            SecondChild = secondChild;
            CrossFadeState = crossFadeState;
            Duration = duration;
            ReverseDuration = reverseDuration ?? duration;
            Alignment = alignment ?? Alignment.Center;
        }

        public Widget FirstChild { get; }
        public Widget SecondChild { get; }
        public CrossFadeState CrossFadeState { get; }
        public float Duration { get; }
        public float ReverseDuration { get; }
        public Alignment Alignment { get; }

        public override State CreateState() => new AnimatedCrossFadeState();
    }

    internal class AnimatedCrossFadeState : ViewState<AnimatedCrossFade>, IAnimatedCrossFadeState
    {
        private readonly Key _firstKey = Key.Of(CrossFadeState.ShowFirst);
        private readonly Key _secondKey = Key.Of(CrossFadeState.ShowSecond);

        private AnimationController _controller;
        private Atom<IState> _firstChild;
        private Atom<IState> _secondChild;
        private Atom<WidgetSize> _size;

        private IAnimation<float> _firstAnimation;
        private IAnimation<float> _secondAnimation;

        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_AnimatedCrossFade");

        public override void InitState()
        {
            base.InitState();

            _firstChild = CreateChild(context =>
            {
                return new FadeTransition(
                    key: _firstKey,
                    child: Widget.FirstChild,
                    opacity: _firstAnimation
                );
            });
            _secondChild = CreateChild(context =>
            {
                return new FadeTransition(
                    key: _secondKey,
                    child: Widget.SecondChild,
                    opacity: _secondAnimation
                );
            });

            _size = Atom.Computed(CalculateSizeInternal);

            var completed = Widget.CrossFadeState == CrossFadeState.ShowSecond;
            _controller = new AnimationController(Widget.Duration, Widget.ReverseDuration, completed);
            _controller.AddStatusListener(ControllerStatusChanged);

            _firstAnimation = _controller.Drive(new FloatTween(1, 0));
            _secondAnimation = _controller.Drive(new FloatTween(0, 1));
        }

        public override void Dispose()
        {
            _controller.RemoveStatusListener(ControllerStatusChanged);

            base.Dispose();
        }

        private void ControllerStatusChanged(AnimationStatus status)
        {
            Tick();
        }

        // TODO: implement ticker
        private void Tick()
        {
            if (!_controller.IsAnimating) return;

            _size.Invalidate();
            Zone.Current.Invoke(Tick);
        }

        public override void DidUpdateWidget(AnimatedCrossFade oldWidget)
        {
            base.DidUpdateWidget(oldWidget);

            if (Math.Abs(oldWidget.Duration - Widget.Duration) > float.Epsilon)
            {
                _controller.Duration = Widget.Duration;
            }

            if (Math.Abs(oldWidget.ReverseDuration - Widget.ReverseDuration) > float.Epsilon)
            {
                _controller.ReverseDuration = Widget.ReverseDuration;
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

        public override WidgetSize CalculateSize() => _size.Value;

        private WidgetSize CalculateSizeInternal()
        {
            if (!_controller.IsAnimating)
            {
                return _controller.IsCompleted ? SecondChild.Size : FirstChild.Size;
            }

            var size1 = FirstChild.Size;
            var size2 = SecondChild.Size;

            var t = _controller.Value;

            float? w = null, h = null;

            if (size1.IsWidthFixed || size2.IsWidthFixed)
            {
                var w1 = size1.IsWidthFixed ? size1.Width : size2.Width;
                var w2 = size2.IsWidthFixed ? size2.Width : size1.Width;
                w = Lerp(w1, w2, t);
            }

            if (size1.IsHeightFixed || size2.IsHeightFixed)
            {
                var h1 = size1.IsHeightFixed ? size1.Height : size2.Height;
                var h2 = size2.IsHeightFixed ? size2.Height : size1.Height;
                h = Lerp(h1, h2, t);
            }

            return new WidgetSize(w, h);
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

        public IState FirstChild => _firstChild.Value;
        public IState SecondChild => _secondChild.Value;
        public Alignment Alignment => Widget.Alignment;
    }
}