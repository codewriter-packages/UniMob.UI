namespace UniMob.UI
{
    using System;
    using UnityEngine;

    public class AnimationController : IAnimation<float>, ILifetimeScope
    {
        private float _prevDeltaTime;

        [Atom] public float Duration { get; set; }
        [Atom] public float ReverseDuration { get; set; }
        [Atom] public float Value { get; private set; }
        [Atom] public AnimationDirection Direction { get; private set; }

        public AnimationStatus Status => Direction == AnimationDirection.Reverse
            ? (Mathf.Approximately(Value, 0f) ? AnimationStatus.Dismissed : AnimationStatus.Reverse)
            : (Mathf.Approximately(Value, 1f) ? AnimationStatus.Completed : AnimationStatus.Forward);

        public bool IsCompleted => Direction == AnimationDirection.Forward && Mathf.Approximately(Value, 1f);
        public bool IsDismissed => Direction == AnimationDirection.Reverse && Mathf.Approximately(Value, 0f);

        public bool IsAnimating =>
            Direction == AnimationDirection.Forward && !Mathf.Approximately(Value, 1f) ||
            Direction == AnimationDirection.Reverse && !Mathf.Approximately(Value, 0f);

        public IAnimation<float> View => this;

        Lifetime ILifetimeScope.Lifetime => Lifetime.Eternal;

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
            Direction = completed ? AnimationDirection.Forward : AnimationDirection.Reverse;
            Value = completed ? 1f : 0f;
        }

        public void Forward()
        {
            Direction = AnimationDirection.Forward;

            if (Mathf.Approximately(Duration, 0))
            {
                Value = 1f;
                return;
            }

            AddAnimationTicker();
        }

        public void Reverse()
        {
            Direction = AnimationDirection.Reverse;

            if (Mathf.Approximately(Duration, 0))
            {
                Value = 0f;
                return;
            }

            AddAnimationTicker();
        }

        public void Complete()
        {
            RemoveAnimationTicker();

            Direction = AnimationDirection.Forward;
            Value = 1f;
        }

        public void Dismiss()
        {
            RemoveAnimationTicker();

            Direction = AnimationDirection.Reverse;
            Value = 0f;
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
                        Value = Mathf.Clamp01(Value + dt / Duration);
                        return;
                    }

                    Value = 1f;
                    break;
                case AnimationDirection.Reverse:
                    if (!Mathf.Approximately(Value, 0f))
                    {
                        Value = Mathf.Clamp01(Value - dt / ReverseDuration);
                        return;
                    }

                    Value = 0f;
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