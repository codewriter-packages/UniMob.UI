#nullable enable
using System;
using System.Collections.Generic;
using TMPro;
using UniMob.UI.Widgets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    internal class RenderText : RenderObject
    {
        private static TextMeshProUGUI? s_textMeshProMeasurer;
        private static TMP_StyleSheet? s_styleSheet;

        private static readonly Dictionary<PreferredSizeCacheKey, Vector2> s_sizeCache = new();


        private readonly TextState _state;

        public RenderText(TextState state)
        {
            _state = state;

            // Ensure static sizer is initialized
            if (s_textMeshProMeasurer == null)
            {
                var prefab = UniMobViewContext.Loader.LoadViewPrefab(_state.View);
                var go = Object.Instantiate(prefab.gameObject);
                go.name = "TextMeshPro Measurer";
                Object.DontDestroyOnLoad(go);
                go.hideFlags = HideFlags.HideAndDontSave;
                s_textMeshProMeasurer = go.GetComponent<UniMobTextMeshProBehaviour>();
                s_styleSheet = TMP_Settings.defaultStyleSheet;
            }
        }

        private Text Widget => (Text) _state.RawWidget;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EditorInitialize()
        {
            if (s_textMeshProMeasurer != null) Object.Destroy(s_textMeshProMeasurer.gameObject);

            s_textMeshProMeasurer = null;
            s_styleSheet = null;
            s_sizeCache.Clear();
        }
#endif

        private Vector2 GetPreferredSize(float maxWidth, float maxHeight)
        {
            if (s_textMeshProMeasurer == null || s_styleSheet == null) return Vector2.zero;


            var key = new PreferredSizeCacheKey
            {
                Text = _state.Value,
                MaxWidth = maxWidth,
                MaxHeight = maxHeight,
                FontSize = _state.FontSize,
                FontWeight = _state.FontWeight,
                Style = _state.Style,
                MaxLines = _state.MaxLines
            };

            if (s_sizeCache.TryGetValue(key, out var cachedSize)) return cachedSize;

            // Configure the static sizer with all properties
            s_textMeshProMeasurer.fontSize = _state.FontSize;
            s_textMeshProMeasurer.fontWeight = _state.FontWeight;
            s_textMeshProMeasurer.textStyle = _state.Style;
            s_textMeshProMeasurer.enableWordWrapping = _state.WrappingEnabled;
            s_textMeshProMeasurer.overflowMode = _state.OverflowMode;


            var fullSize = s_textMeshProMeasurer.GetPreferredValues(_state.Value, maxWidth, maxHeight);

            // The above full size is computed ignoring the max lines property. This is a limitation of TMP_Pro.
            // We recover here by manually computing the line height from the font face info and altering our
            // preferred size. 
            var maxLines = _state.MaxLines;
            if (maxLines is < int.MaxValue and > 0)
            {
                var font = s_textMeshProMeasurer.font;
                var fontScale = s_textMeshProMeasurer.fontSize / font.faceInfo.pointSize;
                var fontLineHeight = font.faceInfo.lineHeight * fontScale;

                var additionalLineSpacing = s_textMeshProMeasurer.lineSpacing;

                // The total height of N lines is (N * font_line_height) + ((N-1) * additional_spacing).
                var maxLinesHeight = maxLines * fontLineHeight + Mathf.Max(0, maxLines - 1) * additionalLineSpacing;

                // 4. The final height is the SMALLER of the two.
                fullSize.y = Mathf.Min(fullSize.y, maxLinesHeight);
            }

            s_sizeCache[key] = fullSize;
            return fullSize;
        }

        protected override Vector2 PerformSizing(LayoutConstraints constraints)
        {
            var widget = Widget;

            var effectiveMaxWidth = Mathf.Min(constraints.MaxWidth, widget.MaxWidth ?? float.PositiveInfinity);
            var effectiveMaxHeight = Mathf.Min(constraints.MaxHeight, widget.MaxHeight ?? float.PositiveInfinity);


            var preferredSize = GetPreferredSize(effectiveMaxWidth, effectiveMaxHeight);

            // The final size is the preferred size, clamped within the original parent constraints.
            return new Vector2(
                Mathf.Clamp(preferredSize.x, constraints.MinWidth, constraints.MaxWidth),
                Mathf.Clamp(preferredSize.y, constraints.MinHeight, constraints.MaxHeight)
            );
        }

        protected override void PerformPositioning()
        {
        }

        public override float GetIntrinsicHeight(float width)
        {
            var widget = Widget;

            var effectiveWidth = Mathf.Min(width, widget.MaxWidth ?? float.PositiveInfinity);
            return GetPreferredSize(effectiveWidth, float.PositiveInfinity).y;
        }

        public override float GetIntrinsicWidth(float height)
        {
            var widget = Widget;

            var effectiveHeight = Mathf.Min(height, widget.MaxHeight ?? float.PositiveInfinity);
            return GetPreferredSize(float.PositiveInfinity, effectiveHeight).x;
        }


        private struct PreferredSizeCacheKey : IEquatable<PreferredSizeCacheKey>
        {
            public string Text { get; set; }
            public float MaxWidth { get; set; }
            public float MaxHeight { get; set; }
            public int FontSize { get; set; }
            public FontWeight FontWeight { get; set; }
            public TMP_Style? Style { get; set; }
            public int MaxLines { get; set; }

            public bool Equals(PreferredSizeCacheKey other)
            {
                return Text == other.Text && MaxWidth.Equals(other.MaxWidth) && MaxHeight.Equals(other.MaxHeight) &&
                       FontSize == other.FontSize && FontWeight == other.FontWeight
                       && Style?.hashCode == other.Style?.hashCode && MaxLines == other.MaxLines;
            }

            public override bool Equals(object? obj)
            {
                return obj is PreferredSizeCacheKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Text, MaxWidth, MaxHeight, FontSize, (int) FontWeight, Style?.hashCode,
                    MaxLines);
            }
        }
    }
}