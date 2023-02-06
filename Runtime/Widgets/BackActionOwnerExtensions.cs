using System;
using JetBrains.Annotations;

namespace UniMob.UI.Widgets
{
    public static class BackActionOwnerExtensions
    {
        [PublicAPI]
        public static TBackActionOwner WithPopOnBack<TBackActionOwner>(this TBackActionOwner owner,
            NavigatorState navigatorState, Func<bool> filter = null)
            where TBackActionOwner : IBackActionOwner
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

            owner.SetBackAction(HandleBack);

            return owner;
        }

        [PublicAPI]
        public static TBaackActionOwner WithPopOnBack<TBaackActionOwner>(this TBaackActionOwner owner,
            NavigatorState navigatorState, object result, Func<bool> filter = null)
            where TBaackActionOwner : IBackActionOwner
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

            owner.SetBackAction(HandleBack);

            return owner;
        }
    }
}