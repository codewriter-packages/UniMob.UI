#nullable enable
using TMPro;
using UniMob.UI.Layout.Internal.RenderObjects;
using UnityEngine;


namespace UniMob.UI.Layout
{
    /// <summary>
    /// Horizontal text alignment options.
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
        public string Value { get; set; } = string.Empty;
        public Color? Color { get; set; }
        public int? FontSize { get; set; }
        public string? StyleName { get; set; }

        public float? MaxWidth { get; set; }
        public float? MaxHeight { get; set; }

        public FontWeight? FontWeight { get; set; }
        public HorizontalTextAlignment? HorizontalTextAlignment { get; set; }
        public bool? WrappingEnabled { get; set; }
        public TextOverflowModes? OverflowMode { get; set; }

        public TMP_StyleSheet? StyleSheet { get; set; }

        public int? MaxLines { get; set; }

        public override State CreateState() => new TextState();

        public override RenderObject CreateRenderObject(BuildContext context, ILayoutState state)
        {
            return new RenderText(this, (TextState) state);
        }
    }

    public class TextState : LayoutState<Text>, IUniMobTextState
    {
        // Exposing all properties for the View, resolving defaults from context.
        public string Value => Widget.Value; // Assuming localization is handled elsewhere now for simplicity
        public Color Color => Widget.Color ?? Color.white; // Or from ThemeProvider
        public int FontSize => Widget.FontSize ?? 14;

        public TMP_StyleSheet StyleSheet => Widget.StyleSheet ?? TMP_Settings.defaultStyleSheet;

        public TMP_Style Style
        {
            get
            {
                if (!string.IsNullOrEmpty(Widget.StyleName) && StyleSheet?.GetStyle(Widget.StyleName) is { } style)
                {
                    return style;
                }

                return TMP_Style.NormalStyle;
            }
        }


        public float MaxWidth => Widget.MaxWidth ?? float.PositiveInfinity;
        public float MaxHeight => Widget.MaxHeight ?? float.PositiveInfinity;
        public int MaxLines => Widget.MaxLines ?? int.MaxValue;

        public FontWeight FontWeight => Widget.FontWeight ?? FontWeight.Regular;

        public HorizontalTextAlignment HorizontalTextAlign =>
            Widget.HorizontalTextAlignment ?? HorizontalTextAlignment.Left;

        public bool WrappingEnabled => Widget.WrappingEnabled ?? true;
        public TextOverflowModes OverflowMode => Widget.OverflowMode ?? TextOverflowModes.Ellipsis;

        public override WidgetViewReference View => WidgetViewReference.Resource("Layout/UniMob.Text");
    }


    public interface IUniMobTextState : IViewState
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