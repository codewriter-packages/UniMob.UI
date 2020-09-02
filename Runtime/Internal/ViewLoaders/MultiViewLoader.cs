
namespace UniMob.UI.Internal.ViewLoaders
{
    internal class MultiViewLoader : IViewLoader
    {
        private readonly IViewLoader[] _loaders;

        public MultiViewLoader(params IViewLoader[] loaders)
        {
            _loaders = loaders;
        }

        public (IView, WidgetViewReference) LoadViewPrefab(IViewState viewState)
        {
            foreach (var loader in _loaders)
            {
                var (view, reference) = loader.LoadViewPrefab(viewState);
                if (view != null)
                {
                    return (view, reference);
                }
            }

            return (null, default);
        }
    }
}