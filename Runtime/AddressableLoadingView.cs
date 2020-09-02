using UnityEngine;

namespace UniMob.UI
{
    public class AddressableLoadingView : MonoBehaviour, IView
    {
        public RectTransform rectTransform => (RectTransform) transform;

        public WidgetViewReference ViewReference { get; set; }

        public BuildContext Context { get; private set; }

        public void SetSource(IViewState source)
        {
            Context = source.Context;
        }

        public void ResetSource()
        {
            Context = null;
        }
    }
}