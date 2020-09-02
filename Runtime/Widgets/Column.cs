using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    public sealed class Column : MultiChildLayoutWidget
    {
        public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;
        public MainAxisAlignment MainAxisAlignment { get; set; } = MainAxisAlignment.Start;

        public AxisSize CrossAxisSize { get; set; } = AxisSize.Min;
        public AxisSize MainAxisSize { get; set; } = AxisSize.Min;

        public override State CreateState() => new ColumnState();
    }

    internal sealed class ColumnState : MultiChildLayoutState<Column>, IColumnState
    {
        private readonly Atom<WidgetSize> _innerSize;

        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_Column");

        public ColumnState()
        {
            _innerSize = Atom.Computed(CalculateInnerSize);
        }

        public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;
        public MainAxisAlignment MainAxisAlignment => Widget.MainAxisAlignment;
        public WidgetSize InnerSize => _innerSize.Value;

        public override WidgetSize CalculateSize()
        {
            var wStretch = Widget.CrossAxisSize == AxisSize.Max;
            var hStretch = Widget.MainAxisSize == AxisSize.Max;

            if (wStretch && hStretch)
            {
                return WidgetSize.Stretched;
            }

            var size = _innerSize.Value;

            float? width = null;
            float? height = null;

            if (size.IsWidthFixed && !wStretch) width = size.Width;
            if (size.IsHeightFixed && !hStretch) height = size.Height;

            return new WidgetSize(width, height);
        }

        private WidgetSize CalculateInnerSize()
        {
            float height = 0;
            float? width = 0;

            foreach (var child in Children)
            {
                var childSize = child.Size;

                if (childSize.IsHeightFixed)
                {
                    height += childSize.Height;
                }

                if (width.HasValue && childSize.IsWidthFixed)
                {
                    width = Mathf.Max(width.Value, childSize.Width);
                }
                else
                {
                    width = null;
                }
            }

            return new WidgetSize(width, height);
        }
    }
}