using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniMob.UI
{
    public class StateProvider
    {
        private readonly Dictionary<Type, Func<State>> _stateFactories = new Dictionary<Type, Func<State>>();

        public static StateProvider Shared { get; } = new StateProvider();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            Shared.Clear();
        }

        public void Register<TWidget>(Func<State> factory) where TWidget : Widget
        {
            var type = typeof(TWidget);

            if (_stateFactories.ContainsKey(type))
            {
                throw new InvalidOperationException($"StateProvider for widget {type.Name} already registered");
            }

            _stateFactories.Add(type, factory);
        }

        public void RegisterUnsafe(Type widgetType, Func<State> factory)
        {
            _stateFactories.Add(widgetType, factory);
        }

        public State Of(Widget w)
        {
            if (!_stateFactories.TryGetValue(w.Type, out var factory))
            {
                throw new InvalidOperationException($"StateProvider for widget {w.Type.Name} not registered");
            }

            return factory();
        }

        public void Clear()
        {
            _stateFactories.Clear();
        }
    }
}