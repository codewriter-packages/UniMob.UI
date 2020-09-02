namespace UniMob.UI.Internal
{
    using System;
    using System.Collections.Generic;

    internal class TriggerStateMachine<TState, TTrigger, TResult>
        where TTrigger : struct
    {
        private readonly Dictionary<Transition, TState> _transitions =
            new Dictionary<Transition, TState>(new Comparer());

        private readonly Dictionary<Transition, Func<TResult, TResult>> _postCallbacks =
            new Dictionary<Transition, Func<TResult, TResult>>(new Comparer());

        private readonly Dictionary<Transition, Func<TResult>> _resultBuilders =
            new Dictionary<Transition, Func<TResult>>(new Comparer());

        public TState State { get; private set; }

        public TriggerStateMachine(TState state) => State = state;

        public void AddTransition(TState state, TTrigger trigger, TState nextState) =>
            _transitions.Add(Transition.Of(state, trigger), nextState);

        public void AddPostCallback(TState state, TTrigger trigger, Func<TResult, TResult> callback) =>
            _postCallbacks.Add(Transition.Of(state, trigger), callback);

        public void AddResultBuilder(TState state, TTrigger trigger, Func<TResult> builder) =>
            _resultBuilders.Add(Transition.Of(state, trigger), builder);

        public bool CanTrigger(TTrigger trigger) =>
            _transitions.ContainsKey(Transition.Of(State, trigger));

        public TResult Trigger(TTrigger trigger)
        {
            var key = Transition.Of(State, trigger);

            if (!_transitions.TryGetValue(key, out var nextState))
                throw new InvalidOperationException($"Trigger {trigger} at {State} not allowed");

            State = nextState;

            var result = _resultBuilders.TryGetValue(key, out var resultBuilder)
                ? resultBuilder()
                : default;

            if (_postCallbacks.TryGetValue(key, out var postCallback))
            {
                result = postCallback(result);
            }

            return result;
        }

        public Builder On(TTrigger trigger) => new Builder(this, trigger);

        public struct Builder
        {
            private readonly TriggerStateMachine<TState, TTrigger, TResult> _machine;
            private readonly TTrigger _trigger;

            public Builder(TriggerStateMachine<TState, TTrigger, TResult> machine, TTrigger trigger)
            {
                _machine = machine;
                _trigger = trigger;
            }

            public Builder Allow(TState from, TState to)
            {
                _machine.AddTransition(from, _trigger, to);
                return this;
            }

            public Builder Allow(TState from, TState to, Func<TResult, TResult> postCallback)
            {
                _machine.AddTransition(from, _trigger, to);
                _machine.AddPostCallback(from, _trigger, postCallback);
                return this;
            }
        }

        private struct Transition
        {
            public TTrigger Trigger;
            public TState State;

            public static Transition Of(TState state, TTrigger trigger) =>
                new Transition {State = state, Trigger = trigger};
        }

        private class Comparer : IEqualityComparer<Transition>
        {
            public bool Equals(Transition x, Transition y) => x.State.Equals(y.State) && x.Trigger.Equals(y.Trigger);
            public int GetHashCode(Transition obj) => obj.State.GetHashCode() ^ obj.Trigger.GetHashCode() << 2;
        }
    }
}