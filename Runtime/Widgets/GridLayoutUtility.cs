using System;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    public static class GridLayoutUtility
    {
        public static readonly GridLayoutDelegate RowLayoutDelegate = data => true;
        public static readonly GridLayoutDelegate ColumnLayoutDelegate = data => false;

        public static readonly GridLayoutDelegate DefaultLayoutDelegate = data =>
            data.lineChildIndex < data.maxLineChildNum &&
            data.lineWidth + data.child.Size.MaxWidth <= data.maxLineWidth;

        public static void LayoutGrid(ref GridLayoutData data, GridLayoutDelegate layoutDelegate, IState[] children)
        {
            var startLineChildIndex = 0;

            while (GridLayoutUtility.LayoutLine(ref data, layoutDelegate, children, startLineChildIndex,
                out var lastLineChildIndex))
            {
                startLineChildIndex = lastLineChildIndex + 1;
            }

            data.gridWidth = Math.Min(data.gridWidth, data.maxLineWidth);
        }

        public static bool LayoutLine(ref GridLayoutData data, GridLayoutDelegate layoutDelegate, IState[] children,
            int childIndex, out int lastChildIndex)
        {
            if (childIndex >= children.Length)
            {
                lastChildIndex = children.Length;
                return false;
            }

            data.lineChildIndex = 0;
            data.lineWidth = 0;
            data.lineHeight = 0;

            for (var i = childIndex; i < children.Length; i++)
            {
                data.child = children[i];

                var childSize = children[i].Size;

                if (float.IsInfinity(childSize.MaxWidth) || float.IsInfinity(childSize.MaxHeight))
                {
                    continue;
                }

                if (i == childIndex || layoutDelegate.Invoke(data))
                {
                    data.childIndex++;
                    data.lineChildIndex++;
                    data.lineWidth += childSize.MaxWidth;
                    data.lineHeight = Math.Max(data.lineHeight, childSize.MaxHeight);
                }
                else
                {
                    break;
                }
            }

            data.lineIndex++;
            data.gridWidth = Math.Max(data.gridWidth, data.lineWidth);
            data.gridHeight += data.lineHeight;

            lastChildIndex = childIndex + data.lineChildIndex - 1;

            return true;
        }
    }

    public delegate bool GridLayoutDelegate(GridLayoutData data);

    public struct GridLayoutData
    {
        public int childrenCount;
        public float maxLineWidth;
        public int maxLineChildNum;

        public IState child;
        public int childIndex;
        public int lineChildIndex;
        public int lineIndex;

        public float gridWidth;
        public float gridHeight;

        public float lineWidth;
        public float lineHeight;

        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }
    }
}