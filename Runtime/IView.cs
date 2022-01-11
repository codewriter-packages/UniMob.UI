using UnityEngine;

namespace UniMob.UI
{
    public interface IView
    {
        // ReSharper disable once InconsistentNaming
        GameObject gameObject { get; }

        // ReSharper disable once InconsistentNaming
        RectTransform rectTransform { get; }

        WidgetViewReference ViewReference { get; set; }

        bool IsDestroyed { get; }

        BuildContext Context { get; }

        void SetSource(IViewState source, bool link);
        void ResetSource();
    }
}