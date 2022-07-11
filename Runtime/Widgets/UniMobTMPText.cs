#if UNIMOB_UI_TMPRO

using UnityEngine;
using TMPro;

namespace UniMob.UI.Widgets
{
    public class UniMobTMPText : StatefulWidget
    {
        public UniMobTMPText(WidgetSize size)
        {
            Size = size;
        }

        public WidgetSize Size { get; }

        public string Value { get; set; }

        public Color Color { get; set; } = Color.black;

        public TextAlignmentOptions TextAlignment { get; set; } = TextAlignmentOptions.TopLeft;

        public int FontSize { get; set; } = 12;

        public override State CreateState() => new UniMobTMPTextState();
    }

    internal class UniMobTMPTextState : ViewState<UniMobTMPText>, IUniMobTMPTextState
    {
        public override WidgetViewReference View { get; }
            = WidgetViewReference.Resource("UniMob.TMPText");

        public string Value => Widget.Value ?? string.Empty;

        public int FontSize => Widget.FontSize;

        public Color Color => Widget.Color;

        public TextAlignmentOptions Alignment => Widget.TextAlignment;

        public override WidgetSize CalculateSize() => Widget.Size;
    }
}

#endif