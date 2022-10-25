using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniMob.UI
{
    public static class StateProvider
    {
        private static readonly Dictionary<Type, Func<State>> StateFactories = new Dictionary<Type, Func<State>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            StateFactories.Clear();
        }

        public static void Register<TWidget>(Func<State> factory)
            where TWidget : Widget
        {
            var type = typeof(TWidget);

            if (StateFactories.ContainsKey(type))
            {
                throw new InvalidOperationException($"StateProvider for widget {type.Name} already registered");
            }

            StateFactories.Add(type, factory);
        }

        public static State Of<T>(T _)
        {
            var type = typeof(T);

            if (!StateFactories.TryGetValue(type, out var factory))
            {
                throw new InvalidOperationException($"StateProvider for widget {type.Name} not registered");
            }

            return factory();
        }
    }
}