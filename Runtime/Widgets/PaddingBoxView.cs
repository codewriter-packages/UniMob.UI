using UniMob.UI.Internal;
using UniMob.UI.Widgets;
using UnityEngine;

[assembly: RegisterComponentViewFactory("$$_PaddingBox",
    typeof(RectTransform), typeof(PaddingBoxView))]

namespace UniMob.UI.Widgets
{
    internal class PaddingBoxView : SingleChildLayoutView<IPaddingBoxState>
    {
        private RectTransform _content;

        protected override void Activate()
        {
            if (_content == null)
            {
                var go = new GameObject("Content", typeof(RectTransform));
                _content = (RectTransform) go.transform;
                _content.SetParent(transform, false);
                _content.anchorMin = Vector2.zero;
                _content.anchorMax = Vector2.one;
            }

            base.Activate();

            UpdateContentPadding();
        }

        protected override void Render()
        {
            UpdateContentPadding();

            base.Render();
        }

        protected override Transform GetContentTransform()
        {
            return _content;
        }

        private void UpdateContentPadding()
        {
            var padding = State.Padding;
            _content.offsetMin = new Vector2(padding.Left, padding.Bottom);
            _content.offsetMax = new Vector2(-padding.Right, -padding.Top);
        }
    }

    internal interface IPaddingBoxState : ISingleChildLayoutState
    {
        RectPadding Padding { get; }
    }
}