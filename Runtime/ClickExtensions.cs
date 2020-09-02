using System;
using JetBrains.Annotations;
using UnityEngine.UI;

namespace UniMob.UI
{
    public static class ClickExtensions
    {
        public static void Click([NotNull] this Button button, [NotNull] Func<Action> call)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));
            if (call == null) throw new ArgumentNullException(nameof(call));

            void Listener()
            {
                var handler = call.Invoke();
                handler?.Invoke();
            }

            button.onClick.AddListener(Listener);
        }

        public static void Click([NotNull] this Button button, [NotNull] Action call)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));
            if (call == null) throw new ArgumentNullException(nameof(call));

            void Listener()
            {
                call.Invoke();
            }

            button.onClick.AddListener(Listener);
        }
    }
}