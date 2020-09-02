using UniMob.UI.Internal;
using UniMob.UI.Widgets;
using UnityEngine;

[assembly: RegisterComponentViewFactory("$$_CompositeTransition",
    typeof(RectTransform), typeof(CanvasGroup), typeof(CompositeTransitionView))]

namespace UniMob.UI.Widgets
{
    internal class CompositeTransitionView : SingleChildLayoutView<ICompositeTransitionState>
    {
        private CanvasGroup _canvasGroup;
        private TransitionTicker<float> _opacity;
        private TransitionTicker<Vector2> _position;
        private TransitionTicker<Vector3> _scale;
        private TransitionTicker<Quaternion> _rotation;

        protected override void Awake()
        {
            base.Awake();

            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected override void Activate()
        {
            base.Activate();

            _opacity = new TransitionTicker<float>(State.Opacity, UpdateOpacity);
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            _opacity.Dispose();
            _position.Dispose();
            _scale.Dispose();
            _rotation.Dispose();
        }

        protected override void Render()
        {
            base.Render();

            _position.Dispose();
            _position = new TransitionTicker<Vector2>(State.Position, UpdatePosition);

            _scale.Dispose();
            _scale = new TransitionTicker<Vector3>(State.Scale, UpdateScale);

            _rotation.Dispose();
            _rotation = new TransitionTicker<Quaternion>(State.Rotation, UpdateRotation);
        }

        private void UpdateOpacity(float val) => _canvasGroup.alpha = val;

        private void UpdateScale(Vector3 val) => ChildView.rectTransform.localScale = val;

        private void UpdatePosition(Vector2 val) =>
            ChildView.rectTransform.anchoredPosition = Vector2.Scale(val, ChildView.rectTransform.rect.size);

        private void UpdateRotation(Quaternion val) => ChildView.rectTransform.rotation = val;
    }

    internal interface ICompositeTransitionState : IFadeTransitionState
    {
        IAnimation<Vector2> Position { get; }
        IAnimation<Vector3> Scale { get; }
        IAnimation<Quaternion> Rotation { get; }
    }
}