namespace UniMob.UI.Widgets
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Navigator : StatefulWidget
    {
        public string InitialRoute { get; }

        public Dictionary<string, Func<Route>> Routes { get; }

        public Navigator(
            string initialRoute,
            Dictionary<string, Func<Route>> routes
        )
        {
            InitialRoute = initialRoute;
            Routes = routes;
        }

        public override State CreateState() => new NavigatorState();

        public static NavigatorState Of(
            BuildContext context,
            bool rootNavigator = false,
            bool nullOk = false
        )
        {
            var navigator = rootNavigator
                ? context.RootAncestorStateOfType<NavigatorState>()
                : context.AncestorStateOfType<NavigatorState>();

            if (!nullOk && navigator == null)
            {
                throw new Exception(
                    "Navigator operation requested with a context that does not include a Navigator.\n" +
                    "The context used to push or pop routes from the Navigator must be that of a " +
                    "widget that is a descendant of a Navigator widget."
                );
            }

            return navigator;
        }

        public static Route Push(BuildContext context, Route route) => Of(context).Push(route);

        public static Task<TResult> Push<TResult>(BuildContext context, Route route) =>
            Of(context).Push<TResult>(route);

        public static Route PushNamed(BuildContext context, string routeName) => Of(context).PushNamed(routeName);

        public static void Pop(BuildContext context) => Of(context).Pop();

        public static Route NewRoot(BuildContext context, Route route) => Of(context).NewRoot(route);

        public static Route NewRootNamed(BuildContext context, string routeName) => Of(context).NewRootNamed(routeName);

        public static Route Replace(BuildContext context, Route route) => Of(context).Replace(route);

        public static Route ReplaceNamed(BuildContext context, string routeName) => Of(context).ReplaceNamed(routeName);

        public static void PopTo(BuildContext context, Route route) => Of(context).PopTo(route);
    }
}