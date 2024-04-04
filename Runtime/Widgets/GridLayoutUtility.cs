using System;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    public static class GridLayoutUtility
    {
        public static readonly GridLayoutDelegate RowLayoutDelegate = (settings, data) => true;
        public static readonly GridLayoutDelegate ColumnLayoutDelegate = (settings, data) => false;
        public static readonly GridLayoutDelegate DefaultLayoutDelegate = (settings, data) => true;

        public static WidgetSize CalculateSize(GridLayoutSettings settings, GridLayoutDelegate layoutDelegate)
        {
            var data = PreLayout(settings);

            while (LayoutLine(settings, ref data, layoutDelegate))
            {
                //
            }

            AfterLayout(settings, ref data);

            return WidgetSize.Fixed(data.gridSize.x, data.gridSize.y);
        }

        public static GridLayoutData PreLayout(GridLayoutSettings settings)
        {
            var data = new GridLayoutData();

            data.gridSize.y += settings.gridPadding.Top;

            return data;
        }

        public static void AfterLayout(GridLayoutSettings settings, ref GridLayoutData data)
        {
            data.gridSize.y += settings.gridPadding.Bottom - settings.spacing.y;

            data.gridSize.x = Math.Min(data.gridSize.x, settings.maxLineWidth);
        }

        public static bool LayoutLine(GridLayoutSettings settings, ref GridLayoutData data,
            GridLayoutDelegate layoutDelegate)
        {
            if (data.gridChildIndex >= settings.children.Length)
            {
                return false;
            }

            data.lineChildIndex = 0;
            data.lineSize = new Vector2(settings.gridPadding.Left, 0);
            data.lineContentCornerPosition = new Vector2(data.lineSize.x, data.gridSize.y);

            var startChildIndex = data.gridChildIndex;

            for (var i = startChildIndex; i < settings.children.Length; i++)
            {
                var child = settings.children[i];
                var childSize = child.Size.GetSizeUnbounded();

                if (float.IsInfinity(childSize.x))
                {
                    if (settings.maxLineChildNum == 1)
                    {
                        childSize.x = 0f;
                        data.gridSize.x = float.PositiveInfinity;
                    }
                    else
                    {
                        Debug.LogError(
                            $"Cannot render multiple horizontally stretched widgets inside Grid.\n" +
                            $"Try to wrap {child.GetType().Name} into another widget of fixed width or to set MaxCrossAxisCount to 1");

                        return false;
                    }
                }

                if (float.IsInfinity(childSize.y))
                {
                    Debug.LogError(
                        $"Cannot render vertically stretched widgets inside Grid.\n" +
                        $"Try to wrap {child.GetType().Name} into another widget of fixed height");

                    return false;
                }

                var newLine = data.lineChildIndex >= settings.maxLineChildNum ||
                              data.lineSize.x + childSize.x > settings.maxLineWidth - settings.gridPadding.Right;

                if (i == startChildIndex || (!newLine && layoutDelegate.Invoke(settings, data)))
                {
                    data.gridChildIndex++;
                    data.lineChildIndex++;

                    data.lineSize.x += childSize.x + settings.spacing.x;
                    data.lineSize.y = Math.Max(data.lineSize.y, childSize.y);
                }
                else
                {
                    break;
                }
            }

            data.lineIndex++;
            data.lineSize.x += settings.gridPadding.Right - settings.spacing.x;

            data.gridSize.x = Math.Max(data.gridSize.x, data.lineSize.x);
            data.gridSize.y += data.lineSize.y + settings.spacing.y;

            return true;
        }
    }

    public delegate bool GridLayoutDelegate(GridLayoutSettings settings, GridLayoutData data);

    public struct GridLayoutSettings
    {
        public IState[] children;
        public float maxLineWidth;
        public int maxLineChildNum;
        public RectPadding gridPadding;
        public Vector2 spacing;
    }

    public struct GridLayoutData
    {
        public int gridChildIndex;
        public int lineChildIndex;
        public int lineIndex;

        public Vector2 gridSize;
        public Vector2 lineSize;

        public Vector2 lineContentCornerPosition;

        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }
    }
}