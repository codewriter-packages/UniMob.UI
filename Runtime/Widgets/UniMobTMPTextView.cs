#if UNIMOB_UI_TMPRO

using UnityEngine;
using TMPro;

namespace UniMob.UI.Widgets
{
    [RequireComponent(typeof(TMP_Text))]
    internal class UniMobTMPTextView : View<IUniMobTMPTextState>
    {
        private TMP_Text _text;

        protected override void Awake()
        {
            base.Awake();

            _text = GetComponent<TMP_Text>();
        }

        protected override void Render()
        {
            _text.text = State.Value;
            _text.color = State.Color;
            _text.fontSize = State.FontSize;
            _text.alignment = State.Alignment;
        }
    }

    internal interface IUniMobTMPTextState : IViewState
    {
        string Value { get; }

        int FontSize { get; }

        Color Color { get; }

        TextAlignmentOptions Alignment { get; }
    }
}

#endif