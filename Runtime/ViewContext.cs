using UniMob.UI.Internal;
using UniMob.UI.Internal.ViewLoaders;

namespace UniMob.UI
{
    public static class ViewContext
    {
        internal static IViewTreeElement CurrentElement;

        internal static IViewLoader Loader = new MultiViewLoader(
            new InternalViewLoader(),
            new BuiltinResourcesViewLoader(),
            new AddressableViewLoader());
    }
}