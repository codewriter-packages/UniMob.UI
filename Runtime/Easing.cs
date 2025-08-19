using UnityEngine;

namespace UniMob.UI
{
    public delegate float Easing(float t, float duration);

    public static class Ease
    {
        private static float NormalizedTime(float t, float d)
        {
            if (d <= 0f) return 1f; // prevent division by zero, an animation with zero duration is considered complete
            return Mathf.Clamp01(t / d);
        }

        public static Easing Combine(Easing first, Easing second)
        {
            return (t, duration) =>
            {
                var n = NormalizedTime(t, duration);
                // The duration for the sub-eases is normalized to 1, as they operate on a 0-1 scale.
                return n < 0.5f
                    ? first(n * 2, 1f) * 0.5f
                    : second((n - 0.5f) * 2, 1f) * 0.5f + 0.5f;
            };
        }

        public static Easing Linear => NormalizedTime;

        // --- SINE ---
        public static Easing InSine => (t, d) => 1 - Mathf.Cos((NormalizedTime(t, d) * Mathf.PI) / 2);
        public static Easing OutSine => (t, d) => Mathf.Sin((NormalizedTime(t, d) * Mathf.PI) / 2);
        public static Easing InOutSine => Combine(InSine, OutSine);

        // --- QUAD (t^2) ---
        public static Easing InQuad => (t, d) =>
        {
            var n = NormalizedTime(t, d);
            return n * n;
        };

        public static Easing OutQuad => (t, d) =>
        {
            var n = NormalizedTime(t, d);
            return 1 - (1 - n) * (1 - n);
        };

        public static Easing InOutQuad => Combine(InQuad, OutQuad);

        // --- CUBIC (t^3) ---
        public static Easing InCubic => (t, d) =>
        {
            var n = NormalizedTime(t, d);
            return n * n * n;
        };

        public static Easing OutCubic => (t, d) =>
        {
            var n = NormalizedTime(t, d);
            var oneMinusN = 1 - n;
            return 1 - oneMinusN * oneMinusN * oneMinusN;
        };

        public static Easing InOutCubic => Combine(InCubic, OutCubic);

        // --- QUART (t^4) ---
        public static Easing InQuart => (t, d) =>
        {
            var n = NormalizedTime(t, d);
            return n * n * n * n;
        };

        public static Easing OutQuart => (t, d) =>
        {
            var n = NormalizedTime(t, d);
            var oneMinusN = 1 - n;
            return 1 - oneMinusN * oneMinusN * oneMinusN * oneMinusN;
        };

        public static Easing InOutQuart => Combine(InQuart, OutQuart);

        // --- QUINT (t^5) ---
        public static Easing InQuint => (t, d) =>
        {
            var n = NormalizedTime(t, d);
            return n * n * n * n * n;
        };

        public static Easing OutQuint => (t, d) =>
        {
            var n = NormalizedTime(t, d);
            // 1 - (1 - n)^5
            var oneMinusN = 1 - n;
            return 1 - oneMinusN * oneMinusN * oneMinusN * oneMinusN * oneMinusN;
        };

        public static Easing InOutQuint => Combine(InQuint, OutQuint);

        // --- EXPO ---
        public static Easing InExpo => (t, d) =>
        {
            var n = NormalizedTime(t, d);
            return Mathf.Approximately(n, 0) ? 0 : Mathf.Pow(2, 10 * n - 10);
        };

        public static Easing OutExpo => (t, d) =>
        {
            var n = NormalizedTime(t, d);
            return Mathf.Approximately(n, 1) ? 1 : 1 - Mathf.Pow(2, -10 * n);
        };

        public static Easing InOutExpo => Combine(InExpo, OutExpo);

        // --- CIRC ---
        public static Easing InCirc => (t, d) => 1 - Mathf.Sqrt(1 - Mathf.Pow(NormalizedTime(t, d), 2));
        public static Easing OutCirc => (t, d) => Mathf.Sqrt(1 - Mathf.Pow(NormalizedTime(t, d) - 1, 2));
        public static Easing InOutCirc => Combine(InCirc, OutCirc);

        // --- BACK ---
        public static Easing InBack => (t, d) =>
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;
            var n = NormalizedTime(t, d);
            return c3 * n * n * n - c1 * n * n;
        };

        public static Easing OutBack => (t, d) =>
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;
            var n = NormalizedTime(t, d);
            return 1 + c3 * Mathf.Pow(n - 1, 3) + c1 * Mathf.Pow(n - 1, 2);
        };

        public static Easing InOutBack => Combine(InBack, OutBack);
    }
}