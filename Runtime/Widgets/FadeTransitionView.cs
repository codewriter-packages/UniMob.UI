using UniMob.UI.Internal;
using UniMob.UI.Widgets;
using UnityEngine;

[assembly: RegisterComponentViewFactory("$$_FadeTransition",
    typeof(RectTransform), typeof(CanvasGroup), typeof(FadeTransitionView))]

namespace UniMob.UI.Widgets
{
    internal class FadeTransitionView : SingleChildLayoutView<IFadeTransitionState>
    {
        private CanvasGroup _canvasGroup;
        private TransitionTicker<float> _opacity;

        protected override void Awake()
        {
            base.Awake();

            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected override void Activate()
        {
            base.Activate();

            _opacity = new TransitionTicker<float>(State.Opacity, val => _canvasGroup.alpha = val);
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            _opacity.Dispose();
        }
    }

    internal interface IFadeTransitionState : ISingleChildLayoutState
    {
        IAnimation<float> Opacity { get; }
    }
}