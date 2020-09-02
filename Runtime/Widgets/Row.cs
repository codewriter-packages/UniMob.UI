using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    public sealed class Row : MultiChildLayoutWidget
    {
        public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;
        public MainAxisAlignment MainAxisAlignment { get; set; } = MainAxisAlignment.Start;

        public AxisSize CrossAxisSize { get; set; } = AxisSize.Min;
        public AxisSize MainAxisSize { get; set; } = AxisSize.Min;

        public override State CreateState() => new RowState();
    }

    internal sealed class RowState : MultiChildLayoutState<Row>, IRowState
    {
        private readonly Atom<WidgetSize> _innerSize;

        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("$$_Row");

        public RowState()
        {
            _innerSize = Atom.Computed(CalculateInnerSize);
        }

        public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;
        public MainAxisAlignment MainAxisAlignment => Widget.MainAxisAlignment;
        public WidgetSize InnerSize => _innerSize.Value;

        public override WidgetSize CalculateSize()
        {
            var wStretch = Widget.MainAxisSize == AxisSize.Max;
            var hStretch = Widget.CrossAxisSize == AxisSize.Max;

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
            float width = 0;
            float? height = 0;

            foreach (var child in Children)
            {
                var childSize = child.Size;

                if (childSize.IsWidthFixed)
                {
                    width += childSize.Width;
                }

                if (height.HasValue && childSize.IsHeightFixed)
                {
                    height = Mathf.Max(height.Value, childSize.Height);
                }
                else
                {
                    height = null;
                }
            }

            return new WidgetSize(width, height);
        }
    }
}