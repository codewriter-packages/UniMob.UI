using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    public class RenderLegacy : RenderObject
    {
        private readonly IViewState _state;

        public RenderLegacy(IViewState state)
        {
            _state = state;
        }

        protected override Vector2 PerformSizing(LayoutConstraints constraints)
        {
            var legacySize = _state.Size;

            return new Vector2(
                Mathf.Clamp(legacySize.MaxWidth, constraints.MinWidth, constraints.MaxWidth),
                Mathf.Clamp(legacySize.MaxHeight, constraints.MinHeight, constraints.MaxHeight)
            );
        }

        protected override void PerformPositioning()
        {
        }

        public override float GetIntrinsicWidth(float height)
        {
            return _state.ViewMaxSize.x;
        }

        public override float GetIntrinsicHeight(float width)
        {
            return _state.ViewMaxSize.y;
        }
    }
}