using TMPro;
using UniMob.UI.Internal;
using UniMob.UI.Layout.Internal.Views;
using UniMob.UI.Widgets;
using UnityEngine;

[assembly: RegisterComponentViewFactory("$$_Layout.Text",
    typeof(LayoutTextView),
    typeof(UniMobTextMeshProBehaviour))]

namespace UniMob.UI.Layout.Internal.Views
{
    [RequireComponent(typeof(UniMobTextMeshProBehaviour))]
    public class LayoutTextView : View<IUniMobTextState>
    {
        [SerializeField] private UniMobTextMeshProBehaviour text;

        protected override void Render()
        {
            if (text == null) return;

            text.text = State.Value;
            text.color = State.Color;
            text.fontSize = State.FontSize;
            text.fontWeight = State.FontWeight;
            text.textStyle = State.Style;
            text.enableWordWrapping = State.WrappingEnabled;
            text.overflowMode = State.OverflowMode;
            text.maxVisibleLines = State.MaxLines;


            text.horizontalAlignment = State.HorizontalTextAlign switch
            {
                HorizontalTextAlignment.Left => HorizontalAlignmentOptions.Left,
                HorizontalTextAlignment.Center => HorizontalAlignmentOptions.Center,
                HorizontalTextAlignment.Right => HorizontalAlignmentOptions.Right,
                HorizontalTextAlignment.Justified => HorizontalAlignmentOptions.Justified,
                HorizontalTextAlignment.Flush => HorizontalAlignmentOptions.Flush,
                HorizontalTextAlignment.Geometry => HorizontalAlignmentOptions.Geometry,
                _ => HorizontalAlignmentOptions.Left
            };

            text.verticalAlignment = VerticalAlignmentOptions.Top;
        }
    }
}