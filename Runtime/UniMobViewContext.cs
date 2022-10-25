using UniMob.UI.Internal;
using UniMob.UI.Internal.ViewLoaders;
using UnityEngine;

namespace UniMob.UI
{
    public static class UniMobViewContext
    {
        public static Sprite DefaultWhiteImage { get; set; }

        internal static IViewTreeElement CurrentElement;

        public static IViewLoader Loader;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            Loader = new MultiViewLoader(
                new InternalViewLoader(),
                new PrefabViewLoader(),
                new BuiltinResourcesViewLoader(),
                new AddressableViewLoader());
        }
    }
}