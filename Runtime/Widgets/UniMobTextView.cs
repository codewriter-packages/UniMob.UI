using UnityEngine;
using UnityEngine.UI;

namespace UniMob.UI.Widgets
{
    [RequireComponent(typeof(Text))]
    internal class UniMobTextView : View<IUniMobTextState>
    {
        private Text _text;

        protected override void Awake()
        {
            base.Awake();

            _text = GetComponent<Text>();
        }

        protected override void Render()
        {
            _text.text = State.Value;
            _text.color = State.Color;
            _text.fontSize = State.FontSize;
            _text.alignment = ToTextAnchor(State.MainAxisAlignment, State.CrossAxisAlignment);
        }

        private static TextAnchor ToTextAnchor(MainAxisAlignment main, CrossAxisAlignment cross)
        {
            switch (main)
            {
                case MainAxisAlignment.Center:
                    return cross == CrossAxisAlignment.Start ? TextAnchor.MiddleLeft
                        : cross == CrossAxisAlignment.Center ? TextAnchor.MiddleCenter
                        : TextAnchor.MiddleRight;

                case MainAxisAlignment.End:
                    return cross == CrossAxisAlignment.Start ? TextAnchor.LowerLeft
                        : cross == CrossAxisAlignment.Center ? TextAnchor.LowerCenter
                        : TextAnchor.LowerRight;

                case MainAxisAlignment.Start:
                    return cross == CrossAxisAlignment.Start ? TextAnchor.UpperLeft
                        : cross == CrossAxisAlignment.Center ? TextAnchor.UpperCenter
                        : TextAnchor.UpperRight;

                default:
                    return TextAnchor.UpperLeft;
            }
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