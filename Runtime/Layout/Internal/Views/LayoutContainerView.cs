using UniMob.UI.Internal;
using UnityEngine;
using UnityEngine.UI;

[assembly: RegisterComponentViewFactory("$$_Layout.ContainerView",
    typeof(UniMob.UI.Layout.Internal.Views.LayoutContainerView))]

namespace UniMob.UI.Layout.Internal.Views
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))]
    internal class LayoutContainerView : SingleChildLayoutView<IContainerState>
    {
        private Image _backgroundImage;

        protected override void Awake()
        {
            base.Awake();
            TryGetComponent(out _backgroundImage);
        }

        protected override void Render()
        {
            base.Render();

            if (_backgroundImage == null) return;
            
            _backgroundImage.sprite = State.BackgroundImage;
            _backgroundImage.color = State.BackgroundColor;
        }
    }

    
}