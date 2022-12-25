using UnityEngine;

namespace UniMob.UI.Widgets
{
    public class UniMobText : StatefulWidget
    {
        public UniMobText()
        {
        }

        public UniMobText(WidgetSize size)
        {
            Size = size;
        }

        public WidgetSize? Size { get; set; }
        public string Value { get; set; }
        public Color Color { get; set; } = Color.black;
        public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Start;
        public MainAxisAlignment MainAxisAlignment { get; set; } = MainAxisAlignment.Start;
        public float MaxCrossAxisExtent { get; set; } = int.MaxValue;

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

        public override WidgetSize CalculateSize()
        {
            if (Widget.Size.HasValue)
            {
                return Widget.Size.Value;
            }

            var prefab = UniMobViewContext.Loader.LoadViewPrefab(View);
            var tmpText = prefab.gameObject.GetComponent<UniMobTextMeshProBehaviour>();

            tmpText.fontSize = FontSize;

            var maxExtent = Widget.MaxCrossAxisExtent;
            var size = tmpText.GetPreferredValues(Value, maxExtent, int.MaxValue);
            size.x = Mathf.Min(maxExtent, size.x);
            return WidgetSize.Fixed(size.x, size.y);
        }
    }
}