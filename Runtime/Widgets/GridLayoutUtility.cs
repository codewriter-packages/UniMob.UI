using System;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    public static class GridLayoutUtility
    {
        private static readonly string[] AxisNames = {"vertically", "horizontally"};

        public static readonly GridLayoutDelegate RowLayoutDelegate = (settings, data) => true;
        public static readonly GridLayoutDelegate ColumnLayoutDelegate = (settings, data) => false;
        public static readonly GridLayoutDelegate DefaultLayoutDelegate = (settings, data) => true;

        public static WidgetSize CalculateSize(GridLayoutSettings settings, GridLayoutDelegate layoutDelegate)
        {
            settings.GetAxis(out var x, out var y);

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
            settings.GetAxis(out var x, out var y);

            var data = new GridLayoutData();

            data.gridSize[y] += settings.gridPadding.GetTop(x);

            return data;
        }

        public static void AfterLayout(GridLayoutSettings settings, ref GridLayoutData data)
        {
            settings.GetAxis(out var x, out var y);

            data.gridSize[y] += settings.gridPadding.GetBottom(x) - settings.spacing[y];

            data.gridSize[x] = Math.Min(data.gridSize[x], settings.maxLineWidth);
        }

        public static bool LayoutLine(GridLayoutSettings settings, ref GridLayoutData data,
            GridLayoutDelegate layoutDelegate)
        {
            if (data.gridChildIndex >= settings.children.Length)
            {
                return false;
            }

            settings.GetAxis(out var x, out var y);

            data.lineChildIndex = 0;
            data.lineSize = Vector2.zero;
            data.lineSize[x] = settings.gridPadding.GetLeft(x);
            data.lineContentCornerPosition = x == 0
                ? new Vector2(data.lineSize.x, data.gridSize.y)
                : new Vector2(data.gridSize.x, data.lineSize.y);

            var startChildIndex = data.gridChildIndex;

            for (var i = startChildIndex; i < settings.children.Length; i++)
            {
                var child = settings.children[i];
                var childSize = child.Size.GetSizeUnbounded();

                if (float.IsInfinity(childSize[x]))
                {
                    if (settings.maxLineChildNum == 1)
                    {
                        childSize[x] = 0f;
                        data.gridSize[x] = float.PositiveInfinity;
                    }
                    else
                    {
                        Debug.LogError(
                            $"Cannot render multiple {AxisNames[x]} stretched widgets inside Grid.\n" +
                            $"Try to wrap {child.GetType().Name} into another widget of fixed size or to set MaxCrossAxisCount to 1");

                        return false;
                    }
                }

                if (float.IsInfinity(childSize[y]))
                {
                    Debug.LogError(
                        $"Cannot render {AxisNames[y]} stretched widgets inside Grid.\n" +
                        $"Try to wrap {child.GetType().Name} into another widget of fixed size");

                    return false;
                }

                var newLine = data.lineChildIndex >= settings.maxLineChildNum ||
                              data.lineSize[x] + childSize[x] >
                              settings.maxLineWidth - settings.gridPadding.GetRight(x);

                if (i == startChildIndex || (!newLine && layoutDelegate.Invoke(settings, data)))
                {
                    data.gridChildIndex++;
                    data.lineChildIndex++;

                    data.lineSize[x] += childSize[x] + settings.spacing[x];
                    data.lineSize[y] = Math.Max(data.lineSize[y], childSize[y]);
                }
                else
                {
                    break;
                }
            }

            data.lineIndex++;
            data.lineSize[x] += settings.gridPadding.GetRight(x) - settings.spacing[x];

            data.gridSize[x] = Math.Max(data.gridSize[x], data.lineSize[x]);
            data.gridSize[y] += data.lineSize[y] + settings.spacing[y];

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
        public int mainAxis;

        public void GetAxis(out int x, out int y)
        {
            x = mainAxis;
            y = 1 - mainAxis;
        }
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