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

            var canvasRenderer = GetComponent<CanvasRenderer>();
            canvasRenderer.cullTransparentMesh = true;

            _backgroundImage = GetComponent<Image>();
        }

        protected override void Render()
        {
            _backgroundImage.sprite = State.BackgroundImage;
            _backgroundImage.color = State.BackgroundColor;

            base.Render();
        }
    }

    internal interface IContainerState : ISingleChildLayoutState
    {
        Sprite BackgroundImage { get; }

        Color BackgroundColor { get; }
    }
}