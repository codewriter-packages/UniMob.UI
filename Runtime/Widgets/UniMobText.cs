using UnityEngine;

namespace UniMob.UI.Widgets
{
    public class UniMobText : StatefulWidget
    {
        public UniMobText(WidgetSize size)
        {
            Size = size;
        }

        public WidgetSize Size { get; }

        public string Value { get; set; }
        public Color Color { get; set; } = Color.black;
        public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;
        public MainAxisAlignment MainAxisAlignment { get; set; } = MainAxisAlignment.Start;

        public int FontSize { get; set; } = 12;

        public override State CreateState() => new UniMobTextState();
    }

    internal class UniMobTextState : ViewState<UniMobText>, IUniMobTextState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("UniMob.Text");

        public string Value => Widget.Value ?? string.Empty;

        public int FontSize => Widget.FontSize;
        public Color Color => Widget.Color;
        public MainAxisAlignment MainAxisAlignment => Widget.MainAxisAlignment;
        public CrossAxisAlignment CrossAxisAlignment => Widget.CrossAxisAlignment;

        public override WidgetSize CalculateSize() => Widget.Size;
    }
}