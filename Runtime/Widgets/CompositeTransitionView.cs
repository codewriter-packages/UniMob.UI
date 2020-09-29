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

        protected override void Awake()
        {
            base.Awake();

            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected override void Render()
        {
            base.Render();

            _canvasGroup.alpha = State.Opacity.Value;

            var childTransform = ChildView.rectTransform;

            childTransform.localScale = State.Scale.Value;
            childTransform.anchoredPosition = Vector2.Scale(State.Position.Value, childTransform.rect.size);
            childTransform.rotation = State.Rotation.Value;
        }
    }

    internal interface ICompositeTransitionState : ISingleChildLayoutState
    {
        IAnimation<float> Opacity { get; }
        IAnimation<Vector2> Position { get; }
        IAnimation<Vector3> Scale { get; }
        IAnimation<Quaternion> Rotation { get; }
    }
}