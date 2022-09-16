using System;
using JetBrains.Annotations;

namespace UniMob.UI.Widgets
{
    public class BackButtonController
    {
        private Func<bool> _handler;

        public void RegisterHandler([NotNull] Func<bool> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public void RegisterHandler([NotNull] NavigatorState handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _handler = handler.HandleBack;
        }

        public bool HandleBack()
        {
            return _handler?.Invoke() ?? true;
        }

        public static TRoute Create<TRoute>([NotNull] Func<BackButtonController, TRoute> func)
            where TRoute : Route
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var backButtonController = new BackButtonController();
            var route = func.Invoke(backButtonController);
            route.BackAction = backButtonController.HandleBack;
            return route;
        }
    }
}