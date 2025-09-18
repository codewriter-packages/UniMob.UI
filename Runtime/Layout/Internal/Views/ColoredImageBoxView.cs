using UniMob.UI.Internal;
using UnityEngine;
using UnityEngine.UI;

[assembly: RegisterComponentViewFactory("$$_Layout.ColoredImageBoxView",
    typeof(UniMob.UI.Layout.Internal.Views.ColoredImageBoxView))]

namespace UniMob.UI.Layout.Internal.Views
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))]
    internal class ColoredImageBoxView : SingleChildLayoutView<IColoredImageBoxState>
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