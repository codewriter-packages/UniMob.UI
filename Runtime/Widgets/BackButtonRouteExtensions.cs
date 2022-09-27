using System;

namespace UniMob.UI.Widgets
{
    public static class BackButtonRouteExtensions
    {
        public static TRoute WithPopOnBack<TRoute>(this TRoute route,
            NavigatorState navigatorState, Func<bool> filter = null)
            where TRoute : Route
        {
            bool HandleBack()
            {
                if (filter == null || filter.Invoke())
                {
                    navigatorState.Pop();
                    return true;
                }

                return false;
            }

            route.BackAction = HandleBack;
            return route;
        }

        public static TRoute WithPopOnBack<TRoute>(this TRoute route,
            NavigatorState navigatorState, object result, Func<bool> filter = null)
            where TRoute : Route
        {
            bool HandleBack()
            {
                if (filter == null || filter.Invoke())
                {
                    navigatorState.Pop(result);
                    return true;
                }

                return false;
            }

            route.BackAction = HandleBack;
            return route;
        }
    }
}