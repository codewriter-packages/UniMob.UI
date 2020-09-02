using System.Collections.Generic;

namespace UniMob.UI
{
    using System;
    using System.Threading.Tasks;
    using UnityEngine;

    public class AnimationController : IAnimation<float>
    {
        private List<Action<AnimationStatus>> _listeners;
        private TaskCompletionSource<object> _completer;
        private float _prevDeltaTime;

        public float Duration { get; set; }
        public float ReverseDuration { get; set; }

        public AnimationStatus Status { get; private set; }

        public bool IsCompleted => Status == AnimationStatus.Completed;
        public bool IsDismissed => Status == AnimationStatus.Dismissed;

        public bool IsAnimating =>
            Status == AnimationStatus.Forward || Status == AnimationStatus.Reverse;

        public float Value { get; private set; }

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

        public void AddStatusListener(Action<AnimationStatus> listener)
        {
            if (_listeners == null)
            {
                _listeners = new List<Action<AnimationStatus>>();
            }

            _listeners.Add(listener);
        }

        public void RemoveStatusListener(Action<AnimationStatus> listener)
        {
            _listeners?.Remove(listener);
        }

        public Task Forward()
        {
            return StartInternal(AnimationStatus.Forward, AnimationStatus.Completed, Duration);
        }

        public Task Reverse()
        {
            return StartInternal(AnimationStatus.Reverse, AnimationStatus.Dismissed, ReverseDuration);
        }

        private Task StartInternal(AnimationStatus status, AnimationStatus endStatus, float d)
        {
            var completer = _completer;
            _completer = null;
            completer?.TrySetResult(null);

            UpdateStatus(status);

            if (Mathf.Approximately(d, 0))
            {
                Value = endStatus == AnimationStatus.Completed ? 1f : 0f;
                UpdateStatus(endStatus);
                return Task.CompletedTask;
            }

            _prevDeltaTime = Time.unscaledDeltaTime;
            _completer = new TaskCompletionSource<object>();

            Zone.Current.RemoveTicker(Tick);
            Zone.Current.AddTicker(Tick);

            return _completer.Task;
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
                    UpdateStatus(AnimationStatus.Completed);
                    break;

                case AnimationStatus.Reverse:
                    UpdateStatus(AnimationStatus.Dismissed);
                    break;
            }

            var completer = _completer;
            _completer = null;
            completer?.TrySetResult(null);
        }

        private void UpdateStatus(AnimationStatus status)
        {
            Status = status;

            if (_listeners != null)
            {
                foreach (var listener in _listeners)
                {
                    listener.Invoke(status);
                }
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

        public void AddStatusListener(Action<AnimationStatus> listener)
            => _controller.AddStatusListener(listener);

        public void RemoveStatusListener(Action<AnimationStatus> listener) =>
            _controller.RemoveStatusListener(listener);
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

        public void AddStatusListener(Action<AnimationStatus> listener)
        {
        }

        public void RemoveStatusListener(Action<AnimationStatus> listener)
        {
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

        void AddStatusListener(Action<AnimationStatus> listener);
        void RemoveStatusListener(Action<AnimationStatus> listener);
    }

    public enum AnimationStatus
    {
        Dismissed = 0,
        Forward = 1,
        Reverse = 2,
        Completed = 3,
    }
}