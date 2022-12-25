using TMPro;
using UnityEngine;

namespace UniMob.UI.Widgets
{
    [RequireComponent(typeof(UniMobTextMeshProBehaviour))]
    internal class UniMobTextView : View<IUniMobTextState>
    {
        [SerializeField] private UniMobTextMeshProBehaviour text;

        protected override void Render()
        {
            text.text = State.Value;
            text.color = State.Color;
            text.fontSize = State.FontSize;
            text.horizontalAlignment = ToHorizontalAlignment(State.CrossAxisAlignment);
            text.verticalAlignment = ToVerticalAlignment(State.MainAxisAlignment);
        }

        private static HorizontalAlignmentOptions ToHorizontalAlignment(CrossAxisAlignment align)
        {
            return align == CrossAxisAlignment.Start ? HorizontalAlignmentOptions.Left
                : align == CrossAxisAlignment.Center ? HorizontalAlignmentOptions.Center
                : HorizontalAlignmentOptions.Right;
        }

        private static VerticalAlignmentOptions ToVerticalAlignment(MainAxisAlignment align)
        {
            return align == MainAxisAlignment.Start ? VerticalAlignmentOptions.Top
                : align == MainAxisAlignment.Center ? VerticalAlignmentOptions.Middle
                : VerticalAlignmentOptions.Bottom;
        }
    }

    internal interface IUniMobTextState : IViewState
    {
        string Value { get; }
        int FontSize { get; }
        Color Color { get; }
        MainAxisAlignment MainAxisAlignment { get; }
        CrossAxisAlignment CrossAxisAlignment { get; }
    }
}