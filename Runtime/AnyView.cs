using UnityEngine;

namespace UniMob.UI
{
    public class AnyView : MonoBehaviour, IView
    {
        public RectTransform rectTransform => (RectTransform) transform;

        public WidgetViewReference ViewReference { get; set; }

        public BuildContext Context { get; private set; }

        public bool IsDestroyed { get; private set; }

        public void SetSource(IViewState source, bool link)
        {
            Context = source.Context;
        }

        public void ResetSource()
        {
            Context = null;
        }

        private void OnDestroy()
        {
            IsDestroyed = true;
        }
    }
}