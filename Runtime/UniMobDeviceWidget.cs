using UnityEngine;
using UnityEngine.UI;

namespace UniMob.UI
{
    public class UniMobDeviceWidget : StatefulWidget
    {
        public UniMobDeviceWidget(Widget child, GameObject root)
        {
            Child = child;
            Root = root;
        }

        public Widget Child { get; }
        public GameObject Root { get; }

        public override State CreateState() => new UniMobDeviceState();
    }

    public class UniMobDeviceState : HocState<UniMobDeviceWidget>
    {
        private RectInt _lastFullArea;
        private RectInt _lastSafeArea;

        [Atom] public RectPadding SafeArea { get; private set; }

        public override Widget Build(BuildContext context)
        {
            return Widget.Child;
        }

        public override void InitState()
        {
            base.InitState();

            RefreshSafeArea();

            Zone.Current.AddTicker(Tick);
        }

        public override void Dispose()
        {
            Zone.Current.RemoveTicker(Tick);

            base.Dispose();
        }

        private void Tick()
        {
            if (!GetFullArea().Equals(_lastFullArea) || !GetSafeArea().Equals(_lastSafeArea))
            {
                RefreshSafeArea();
            }
        }

        public void RefreshSafeArea()
        {
            var invScale = 1f / GetScale();

            _lastFullArea = GetFullArea();
            _lastSafeArea = GetSafeArea();

            var left = (_lastSafeArea.xMin - _lastFullArea.xMin) * invScale;
            var right = (_lastFullArea.xMax - _lastSafeArea.xMax) * invScale;
            var bottom = (_lastSafeArea.yMin - _lastFullArea.yMin) * invScale;
            var top = (_lastFullArea.yMax - _lastSafeArea.yMax) * invScale;

            SafeArea = new RectPadding(left, right, top, bottom);
        }

        private float GetScale()
        {
            var canvasScaler = Widget.Root.GetComponentInParent<CanvasScaler>();
            if (canvasScaler == null)
            {
                return 1f;
            }

            var screen = new Vector2(Screen.width, Screen.height);
            var reference = canvasScaler.referenceResolution;

            switch (canvasScaler.screenMatchMode)
            {
                case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
                    return Mathf.Pow(2f, Mathf.Lerp(
                        Mathf.Log(screen.x / reference.x, 2f),
                        Mathf.Log(screen.y / reference.y, 2f), canvasScaler.matchWidthOrHeight));

                case CanvasScaler.ScreenMatchMode.Expand:
                    return Mathf.Min(screen.x / reference.x, screen.y / reference.y);

                case CanvasScaler.ScreenMatchMode.Shrink:
                    return Mathf.Max(screen.x / reference.x, screen.y / reference.y);

                default:
                    return 1f;
            }
        }

        private static RectInt GetFullArea()
        {
            return new RectInt(0, 0, Screen.width, Screen.height);
        }

        private static RectInt GetSafeArea()
        {
            var area = Screen.safeArea;
            return new RectInt((int) area.x, (int) area.y, (int) area.width, (int) area.height);
        }

        public static UniMobDeviceState Of(BuildContext context)
        {
            return context.AncestorStateOfType<UniMobDeviceState>();
        }
    }
}