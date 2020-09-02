using UniMob.UI.Internal;
using UniMob.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

[assembly: RegisterComponentViewFactory("$$_Container",
    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ContainerView))]

namespace UniMob.UI.Widgets
{
    internal sealed class ContainerView : SingleChildLayoutView<IContainerState>
    {
        private Image _backgroundImage;

        protected override void Awake()
        {
            base.Awake();

            _backgroundImage = GetComponent<Image>();
        }

        protected override void Render()
        {
            var backgroundColor = State.BackgroundColor;
            var transparent = backgroundColor == Color.clear;

            _backgroundImage.enabled = !transparent;
            _backgroundImage.color = backgroundColor;

            base.Render();
        }
    }

    internal interface IContainerState : ISingleChildLayoutState
    {
        Color BackgroundColor { get; }
    }
}