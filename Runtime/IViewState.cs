using UnityEngine;

namespace UniMob.UI
{
    public interface IViewState : IState
    {
        WidgetViewReference View { get; }

        void DidViewMount(IView view);
        void DidViewUnmount(IView view);

        /// <summary>
        /// Size of the unity view prefab.
        /// </summary>
        Vector2 ViewMaxSize { get; }
    }
}