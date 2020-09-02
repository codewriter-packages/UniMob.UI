using System;
using System.Collections.Generic;

namespace UniMob.UI
{
    public static class StateProvider
    {
        private static readonly Dictionary<Type, Func<State>> StateFactories = new Dictionary<Type, Func<State>>();

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