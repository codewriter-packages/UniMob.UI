namespace UniMob.UI
{
    using System;
    using UnityEngine;

    public class AnimationController : IAnimation<float>
    {
        private float _prevDeltaTime;

        private readonly MutableAtom<AnimationDirection> _direction = Atom.Value(AnimationDirection.Reverse);

        private readonly MutableAtom<float> _duration = Atom.Value(0f);
        private readonly MutableAtom<float> _reverseDuration = Atom.Value(0f);
        private readonly MutableAtom<float> _value = Atom.Value(0f);

        public float Duration
        {
            get => _duration.Value;
            set => _duration.Value = value;
        }

        public float ReverseDuration
        {
            get => _reverseDuration.Value;
            set => _reverseDuration.Value = value;
        }

        public float Value => _value.Value;

        public AnimationDirection Direction => _direction.Value;

        public AnimationStatus Status => Direction == AnimationDirection.Reverse
            ? (Mathf.Approximately(Value, 0f) ? AnimationStatus.Dismissed : AnimationStatus.Reverse)
            : (Mathf.Approximately(Value, 1f) ? AnimationStatus.Completed : AnimationStatus.Forward);

        public bool IsCompleted => Direction == AnimationDirection.Forward && Mathf.Approximately(Value, 1f);
        public bool IsDismissed => Direction == AnimationDirection.Reverse && Mathf.Approximately(Value, 0f);

        public bool IsAnimating =>
            Direction == AnimationDirection.Forward && !Mathf.Approximately(Value, 1f) ||
            Direction == AnimationDirection.Reverse && !Mathf.Approximately(Value, 0f);

        public IAnimation<float> View => this;

        public AnimationController(float duration, float? reverseDuration = null, bool completed = false)
        {
            if (duration < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "Must be positive or zero");
            }

            if (reverseDuration.HasValue && reverseDuration < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(reverseDuration), reverseDuration,
                    "Must positive or zero");
            }

            Duration = duration;
            ReverseDuration = reverseDuration ?? duration;
            _direction.Value = completed ? AnimationDirection.Forward : AnimationDirection.Reverse;
            _value.Value = completed ? 1f : 0f;
        }

        public void Forward()
        {
            _direction.Value = AnimationDirection.Forward;

            if (Mathf.Approximately(Duration, 0))
            {
                _value.Value = 1f;
                return;
            }

            AddAnimationTicker();
        }

        public void Reverse()
        {
            _direction.Value = AnimationDirection.Reverse;

            if (Mathf.Approximately(Duration, 0))
            {
                _value.Value = 0f;
                return;
            }

            AddAnimationTicker();
        }

        public void Complete()
        {
            RemoveAnimationTicker();

            _direction.Value = AnimationDirection.Forward;
            _value.Value = 1f;
        }

        public void Dismiss()
        {
            RemoveAnimationTicker();

            _direction.Value = AnimationDirection.Reverse;
            _value.Value = 0f;
        }

        private void AddAnimationTicker()
        {
            _prevDeltaTime = Time.unscaledDeltaTime;

            Zone.Current.RemoveTicker(Tick);
            Zone.Current.AddTicker(Tick);
        }

        private void RemoveAnimationTicker()
        {
            Zone.Current.RemoveTicker(Tick);
        }

        private void Tick()
        {
            var nextDeltaTime = Time.unscaledDeltaTime;
            var dt = Mathf.Min(nextDeltaTime * 0.8f + _prevDeltaTime * 0.2f, 1 / 5f);
            _prevDeltaTime = nextDeltaTime;

            switch (Direction)
            {
                case AnimationDirection.Forward:
                    if (!Mathf.Approximately(Value, 1f))
                    {
                        _value.Value = Mathf.Clamp01(_value.Value + dt / Duration);
                        return;
                    }

                    _value.Value = 1f;
                    break;
                case AnimationDirection.Reverse:
                    if (!Mathf.Approximately(Value, 0f))
                    {
                        _value.Value = Mathf.Clamp01(_value.Value - dt / ReverseDuration);
                        return;
                    }

                    _value.Value = 0f;
                    break;
            }


            RemoveAnimationTicker();
        }
    }

    internal class DrivenAnimation<T> : IAnimation<T>
    {
        private readonly IAnimation<float> _controller;
        private readonly IAnimatable<T> _tween;

        public DrivenAnimation(IAnimation<float> controller, IAnimatable<T> tween)
        {
            _controller = controller;
            _tween = tween;
        }

        public bool IsAnimating => _controller.IsAnimating;
        public bool IsCompleted => _controller.IsCompleted;
        public bool IsDismissed => _controller.IsDismissed;
        public AnimationStatus Status => _controller.Status;
        public T Value => _tween.Transform(_controller.Value);
    }

    public static class AnimatableExtensions
    {
        public static IAnimation<T> Drive<T>(this IAnimation<float> controller,
            IAnimatable<T> tween)
        {
            return tween.Animate(controller);
        }

        public static IAnimation<T> Animate<T>(this IAnimatable<T> parent,
            IAnimation<float> controller)
        {
            return new DrivenAnimation<T>(controller, parent);
        }
    }

    public sealed class CurvedAnimation : IAnimation<float>
    {
        private readonly IAnimation<float> _controller;
        private readonly Func<float, float> _curve;
        private readonly Func<float, float> _reverseCurve;

        public CurvedAnimation(IAnimation<float> controller,
            Func<float, float> curve, Func<float, float> reverseCurve = null)
        {
            _controller = controller;
            _curve = curve ?? (t => t);
            _reverseCurve = reverseCurve ?? _curve;
        }

        public bool IsAnimating => _controller.IsAnimating;
        public bool IsCompleted => _controller.IsCompleted;
        public bool IsDismissed => _controller.IsDismissed;
        public AnimationStatus Status => _controller.Status;

        public float Value
        {
            get
            {
                switch (_controller.Status)
                {
                    case AnimationStatus.Forward:
                    case AnimationStatus.Completed:
                        return _curve(_controller.Value);

                    case AnimationStatus.Reverse:
                    case AnimationStatus.Dismissed:
                        return _reverseCurve(_controller.Value);
                    default:
                        return _controller.Value;
                }
            }
        }
    }

    public sealed class ConstAnimation<T> : IAnimation<T>
    {
        public bool IsAnimating => false;
        public bool IsCompleted => true;
        public bool IsDismissed => false;
        public AnimationStatus Status => AnimationStatus.Completed;
        public T Value { get; }

        public ConstAnimation(T value)
        {
            Value = value;
        }

        public static implicit operator ConstAnimation<T>(T value) => new ConstAnimation<T>(value);
    }

    public interface IAnimatable<out T>
    {
        T Transform(float t);
    }

    public interface IAnimation<out T>
    {
        bool IsAnimating { get; }
        bool IsCompleted { get; }
        bool IsDismissed { get; }

        AnimationStatus Status { get; }

        T Value { get; }
    }

    public enum AnimationStatus
    {
        Dismissed = 0,
        Forward = 1,
        Reverse = 2,
        Completed = 3,
    }

    public enum AnimationDirection
    {
        Forward,
        Reverse,
    }
}