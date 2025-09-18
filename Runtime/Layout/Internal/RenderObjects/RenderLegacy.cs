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

            return constraints.Constrain(legacySize.GetSizeUnbounded());
        }

        protected override void PerformPositioning()
        {
        }

        public override float GetIntrinsicWidth(float height)
        {
            return _state.Size.MaxWidth;
        }

        public override float GetIntrinsicHeight(float width)
        {
            return _state.Size.MaxHeight;
        }
    }
}