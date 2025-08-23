using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    public class RenderEmpty : RenderObject
    {
        public static readonly RenderEmpty Shared = new RenderEmpty();

        protected override Vector2 PerformSizing(LayoutConstraints contextConstraints)
        {
            return Vector2.zero;
        }

        protected override void PerformPositioning()
        {
        }

        public override float GetIntrinsicWidth(float height)
        {
            return 0f;
        }

        public override float GetIntrinsicHeight(float width)
        {
            return 0f;
        }
    }
}