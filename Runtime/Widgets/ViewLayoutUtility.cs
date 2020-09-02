using UnityEngine;

namespace UniMob.UI.Widgets
{
    public static class ViewLayoutUtility
    {
        public static void SetLayout(RectTransform rt, LayoutData layoutData)
        {
            var size = layoutData.Size;
            var sizeDelta = new Vector2(
                size.IsWidthFixed ? size.Width : 0,
                size.IsHeightFixed ? size.Height : 0
            );

            var corner = layoutData.Corner;
            var anchor = layoutData.Alignment.ToAnchor();
            var anchorMin = anchor;
            var anchorMax = anchor;

            if (size.IsWidthStretched)
            {
                corner = corner.WithCenterX();
                anchorMin.x = 0;
                anchorMax.x = 1;
            }

            if (size.IsHeightStretched)
            {
                corner = corner.WithCenterY();
                anchorMin.y = 0;
                anchorMax.y = 1;
            }

            rt.anchoredPosition = CornerPositionToAnchored(layoutData.CornerPosition, rt.pivot, sizeDelta, corner);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = sizeDelta;
        }

        private static Vector2 CornerPositionToAnchored(Vector2 position, Vector2 pivot, Vector2 size, Alignment corner)
        {
            return new Vector2(position.x, -position.y) +
                   new Vector2(size.x * pivot.x * -corner.X, -size.y * (1f - pivot.y) * -corner.Y);
        }
    }

    public struct LayoutData
    {
        public WidgetSize Size;
        public Vector2 CornerPosition;
        public Alignment Corner;
        public Alignment Alignment;
    }
}