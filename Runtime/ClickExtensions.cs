using System;
using JetBrains.Annotations;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UniMob.UI
{
    public static class ClickExtensions
    {
        public static void Click([NotNull] this Button button, [NotNull] Func<Action> call)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));

            Bind(button.onClick, call);
        }

        public static void Click([NotNull] this Button button, [NotNull] Action call)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));

            Bind(button.onClick, call);
        }

        public static void Bind([NotNull] this UnityEvent unityEvent, [NotNull] Func<Action> call)
        {
            if (unityEvent == null) throw new ArgumentNullException(nameof(unityEvent));
            if (call == null) throw new ArgumentNullException(nameof(call));

            void Listener()
            {
                using (Atom.NoWatch)
                {
                    var handler = call.Invoke();
                    handler?.Invoke();
                }
            }

            unityEvent.AddListener(Listener);
        }

        public static void Bind([NotNull] this UnityEvent unityEvent, [NotNull] Action call)
        {
            if (unityEvent == null) throw new ArgumentNullException(nameof(unityEvent));
            if (call == null) throw new ArgumentNullException(nameof(call));

            void Listener()
            {
                using (Atom.NoWatch)
                {
                    call.Invoke();
                }
            }

            unityEvent.AddListener(Listener);
        }

        public static void Bind<T>([NotNull] this UnityEvent<T> unityEvent, [NotNull] Func<Action<T>> call)
        {
            if (unityEvent == null) throw new ArgumentNullException(nameof(unityEvent));
            if (call == null) throw new ArgumentNullException(nameof(call));

            void Listener(T value)
            {
                using (Atom.NoWatch)
                {
                    var handler = call.Invoke();
                    handler?.Invoke(value);
                }
            }

            unityEvent.AddListener(Listener);
        }

        public static void Bind<T>([NotNull] this UnityEvent<T> unityEvent, [NotNull] Action<T> call)
        {
            if (unityEvent == null) throw new ArgumentNullException(nameof(unityEvent));
            if (call == null) throw new ArgumentNullException(nameof(call));

            void Listener(T value)
            {
                using (Atom.NoWatch)
                {
                    call.Invoke(value);
                }
            }

            unityEvent.AddListener(Listener);
        }
    }
}