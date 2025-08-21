#nullable enable
using JetBrains.Annotations;
using TMPro;
using UniMob.UI.Layout.Internal.RenderObjects;
using UnityEngine;

namespace UniMob.UI.Layout
{
    /// <summary>
    ///     Horizontal text alignment options.
    /// </summary>
    public enum HorizontalTextAlignment
    {
        Left = 0x1,
        Center = 0x2,
        Right = 0x4,
        Justified = 0x8,
        Flush = 0x10,
        Geometry = 0x20
    }


    public class Text : LayoutWidget
    {
        public WidgetViewReference? ViewReference { get; set; }

        public string Value { get; set; } = string.Empty;
        public Color? Color { get; set; }
        public int? FontSize { get; set; }
        public string? StyleName { get; set; }

        public FontWeight? FontWeight { get; set; }
        public HorizontalTextAlignment? HorizontalTextAlignment { get; set; }
        public bool? WrappingEnabled { get; set; }
        public TextOverflowModes? OverflowMode { get; set; }

        public TMP_StyleSheet? StyleSheet { get; set; }

        public int? MaxLines { get; set; }

        /// <summary>
        ///     Forces the text to a fixed square size. Useful for icons or fixed-size text elements.
        ///     If not set, the text will size itself based on its content.
        /// </summary>
        public float? FixedSize { get; set; }

        public override State CreateState()
        {
            return new TextState();
        }

        public override RenderObject CreateRenderObject(BuildContext context, ILayoutState state)
        {
            return new RenderText((TextState) state);
        }
    }

    public class TextState : LayoutState<Text>, ITextState
    {
        private TMP_StyleSheet StyleSheet => Widget.StyleSheet ?? TMP_Settings.defaultStyleSheet;

        // Exposing all properties for the View, resolving defaults from context.
        public string Value => Widget.Value;
        public Color Color => Widget.Color ?? Color.white;
        public int FontSize => Widget.FontSize ?? 14;
        
        public float? FixedSize => Widget.FixedSize;
        
        public TMP_Style Style
        {
            get
            {
                if (!string.IsNullOrEmpty(Widget.StyleName) && StyleSheet?.GetStyle(Widget.StyleName) is { } style)
                    return style;

                return TMP_Style.NormalStyle;
            }
        }


        public int MaxLines => Widget.MaxLines ?? int.MaxValue;

        public FontWeight FontWeight => Widget.FontWeight ?? FontWeight.Regular;

        public HorizontalTextAlignment HorizontalTextAlign =>
            Widget.HorizontalTextAlignment ?? HorizontalTextAlignment.Left;

        public bool WrappingEnabled => Widget.WrappingEnabled ?? true;
        public TextOverflowModes OverflowMode => Widget.OverflowMode ?? TextOverflowModes.Ellipsis;

        public override WidgetViewReference View =>
            Widget.ViewReference ?? WidgetViewReference.Resource("Layout/UniMob.Text");
    }


    public interface ITextState : IViewState
    {
        string Value { get; }
        Color Color { get; }
        
        int FontSize { get; }
        
        TMP_Style Style { get; }
        
        FontWeight FontWeight { get; }
        HorizontalTextAlignment HorizontalTextAlign { get; }

        bool WrappingEnabled { get; }
        TextOverflowModes OverflowMode { get; }

        int MaxLines { get; }
    }
}