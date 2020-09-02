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

        BuildContext Context { get; }

        void SetSource(IViewState source);
        void ResetSource();
    }
}