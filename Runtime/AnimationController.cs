namespace UniMob.UI
{
    using System;
    using UnityEngine;

    public class AnimationController : IAnimation<float>
    {
        private float _prevDeltaTime;

        private MutableAtom<float> _duration = Atom.Value("AnimationController::Duration", 0f);
        private MutableAtom<float> _reverseDuration = Atom.Value("AnimationController::ReverseDuration", 0f);
        private MutableAtom<float> _value = Atom.Value("AnimationController::Value", 0f);

        private MutableAtom<AnimationStatus> _status = Atom.Value("AnimationController::Duration",
            AnimationStatus.Dismissed);

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

        public float Value
        {
            get => _value.Value;
            private set => _value.Value = value;
        }

        public AnimationStatus Status
        {
            get => _status.Value;
            private set => _status.Value = value;
        }

        public bool IsCompleted => Status == AnimationStatus.Completed;
        public bool IsDismissed => Status == AnimationStatus.Dismissed;

        public bool IsAnimating => Status == AnimationStatus.Forward || Status == AnimationStatus.Reverse;

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
            Status = completed ? AnimationStatus.Completed : AnimationStatus.Dismissed;
            Value = completed ? 1f : 0f;
        }

        public void Forward()
        {
            StartInternal(AnimationStatus.Forward, AnimationStatus.Completed, Duration);
        }

        public void Reverse()
        {
            StartInternal(AnimationStatus.Reverse, AnimationStatus.Dismissed, ReverseDuration);
        }

        private void StartInternal(AnimationStatus status, AnimationStatus endStatus, float d)
        {
            Status = status;

            if (Mathf.Approximately(d, 0))
            {
                Value = endStatus == AnimationStatus.Completed ? 1f : 0f;
                Status = endStatus;
                return;
            }

            _prevDeltaTime = Time.unscaledDeltaTime;

            Zone.Current.RemoveTicker(Tick);
            Zone.Current.AddTicker(Tick);
        }

        private void Tick()
        {
            var nextDeltaTime = Time.unscaledDeltaTime;
            var dt = Mathf.Min(nextDeltaTime * 0.8f + _prevDeltaTime * 0.2f, 1 / 5f);
            _prevDeltaTime = nextDeltaTime;

            switch (Status)
            {
                case AnimationStatus.Forward:
                    if (!Mathf.Approximately(Value, 1f))
                    {
                        Value += dt / Duration;
                        Value = Mathf.Clamp01(Value);
                        return;
                    }

                    Value = 1f;
                    break;

                case AnimationStatus.Reverse:
                    if (!Mathf.Approximately(Value, 0f))
                    {
                        Value -= dt / ReverseDuration;
                        Value = Mathf.Clamp01(Value);
                        return;
                    }

                    Value = 0f;
                    break;
            }

            Zone.Current.RemoveTicker(Tick);

            switch (Status)
            {
                case AnimationStatus.Forward:
                    Status = AnimationStatus.Completed;
                    break;

                case AnimationStatus.Reverse:
                    Status = AnimationStatus.Dismissed;
                    break;
            }
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
}