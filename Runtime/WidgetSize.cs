using System;

namespace UniMob.UI
{
    public struct WidgetSize
    {
        private float? _width;
        private float? _height;

        public bool IsWidthFixed => _width.HasValue;
        public bool IsHeightFixed => _height.HasValue;

        public bool IsWidthStretched => !_width.HasValue;
        public bool IsHeightStretched => !_height.HasValue;

        public float Width => _width ?? throw new InvalidOperationException("Cannot read width at stretch");
        public float Height => _height ?? throw new InvalidOperationException("Cannot read height at stretch");

        public WidgetSize(float? width, float? height)
        {
            _width = width;
            _height = height;
        }

        public WidgetSize WithWidth(float? width) => new WidgetSize(width, _height);
        public WidgetSize WithHeight(float? height) => new WidgetSize(_width, height);

        public static WidgetSize Fixed(float width, float height) => new WidgetSize(width, height);
        public static WidgetSize FixedWidth(float width) => new WidgetSize(width, null);
        public static WidgetSize FixedHeight(float height) => new WidgetSize(null, height);
        public static WidgetSize Stretched => new WidgetSize(null, null);
    }
}