using System;
using System.Collections;
using UniMob.UI.Internal;
using UniMob.UI.Layout.Internal.RenderObjects;
using UnityEngine;
using UnityEngine.UI;

namespace UniMob.UI.Layout.Internal.Views
{
    [RequireComponent(typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D))]
    internal class ScrollListView : View<IScrollingListState>
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private RectMask2D rectMask;
        private bool _isUpdatingFromController;

        private ViewMapperBase _mapper;
        private RectTransform _rectTransform;

        protected override void Awake()
        {
            base.Awake();
            _mapper = new PooledViewMapper(contentRoot);
            _rectTransform = (RectTransform) transform;

            if (scrollRect == null) TryGetComponent(out scrollRect);
            if (contentRoot == null) contentRoot = scrollRect.content;
            if (rectMask == null) TryGetComponent(out rectMask);

            if (scrollRect.horizontal && scrollRect.vertical)
                throw new InvalidOperationException(
                    "ScrollRect cannot be both horizontal and vertical at the same time." +
                    " Please set either horizontal or vertical to true, but not both.");

            EnsurePivotAndAnchorsAreConsistentWithDirection(scrollRect.horizontal && !scrollRect.vertical);
            // The onValueChanged listener is now connected with a guard flag.
            scrollRect.onValueChanged.AddListener(OnScrollPositionChanged);
        }


        private void OnDrawGizmos()
        {
            if (!HasState || State.RenderObject is not RenderSliverList renderObject || !Application.isPlaying) return;

            // --- 1. Get World Space Origin of the Content ---
            var contentCorners = new Vector3[4];
            contentRoot.GetWorldCorners(contentCorners);
            // contentCorners[1] is the top-left corner of the content in world space
            var contentWorldTopLeft = (Vector2) contentCorners[1];

            // --- 2. Draw the Static Viewport (Green) ---
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            var viewportSize = State.ViewportSize;
            var rectTransform = (RectTransform) transform;
            var viewportCorners = new Vector3[4];
            rectTransform.GetWorldCorners(viewportCorners);
            var viewportWorldCenter = (Vector2) viewportCorners[1] +
                                      new Vector2(viewportSize.x * 0.5f, -viewportSize.y * 0.5f);
            Gizmos.DrawCube(viewportWorldCenter, viewportSize);

            // --- 3. Draw the Visible Children (Blue) ---
            Gizmos.color = new Color(0, 0, 1, 0.2f);
            var childrenLayout = renderObject.ChildrenLayout;
            foreach (var layout in childrenLayout)
            {
                var layoutPos = layout.CornerPosition.Value;
                var layoutSize = layout.Size;

                // The layoutPos is relative to the content's top-left, with Y-down.
                // We add it to the content's world-space top-left to find the child's world position.
                var childWorldTopLeft = contentWorldTopLeft + new Vector2(layoutPos.x, -layoutPos.y);

                // Gizmos.DrawCube requires a center position, so we offset by half the size.
                var childWorldCenter = childWorldTopLeft + new Vector2(layoutSize.x * 0.5f, -layoutSize.y * 0.5f);

                Gizmos.DrawCube(childWorldCenter, layoutSize);
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            if (!HasState || State.StateLifetime.IsDisposed) return;
            base.OnRectTransformDimensionsChange();
            State.ViewportSize = _rectTransform.rect.size;
        }

        protected override void Activate()
        {
            base.Activate();

            // Set the initial scroll position when the view becomes active.
            var isHorizontal = State.Axis == Axis.Horizontal;
            State.ViewportSize = _rectTransform.rect.size;
            if (isHorizontal)
                scrollRect.horizontalNormalizedPosition = State.ScrollController.NormalizedValue;
            else
                scrollRect.verticalNormalizedPosition = 1f - State.ScrollController.NormalizedValue;

            Atom.Reaction(StateLifetime, () =>
            {
                var controllerValue = State.ScrollController.NormalizedValue;

                var currentScrollRectValue = isHorizontal
                    ? scrollRect.horizontalNormalizedPosition
                    : 1 - scrollRect.verticalNormalizedPosition;

                if (Mathf.Abs(currentScrollRectValue - controllerValue) > 0.001f)
                {
                    _isUpdatingFromController = true;
                    if (isHorizontal)
                        scrollRect.horizontalNormalizedPosition = controllerValue;
                    else
                        scrollRect.verticalNormalizedPosition = 1 - controllerValue;

                    // Use Zone.NextFrame to reset the flag, as you correctly suggested.
                    Zone.Current.NextFrame(() => _isUpdatingFromController = false);
                }
            });
        }

        private void EnsurePivotAndAnchorsAreConsistentWithDirection(bool isHorizontal)
        {
            var targetPivot = isHorizontal
                ? new Vector2(0, contentRoot.pivot.y)
                // For vertical, set pivot to the top.
                : new Vector2(contentRoot.pivot.x, 1);


            var targetAnchorMin = isHorizontal ? new Vector2(0, 0) : new Vector2(0, 1);
            var targetAnchorMax = isHorizontal ? new Vector2(0, 1) : new Vector2(1, 1);

            if (contentRoot.pivot == targetPivot && contentRoot.anchorMin == targetAnchorMin &&
                contentRoot.anchorMax == targetAnchorMax) return;

            // To prevent the content from visually jumping when the pivot changes,
            // we calculate the positional offset caused by the pivot shift
            // and apply an opposite adjustment to the anchoredPosition.
            var originalPivot = contentRoot.pivot;
            var originalSize = contentRoot.rect.size;

            // Apply new anchors and pivot
            contentRoot.anchorMin = targetAnchorMin;
            contentRoot.anchorMax = targetAnchorMax;
            contentRoot.pivot = targetPivot;

            var pivotChange = contentRoot.pivot - originalPivot;
            var positionChange = new Vector2(pivotChange.x * originalSize.x, pivotChange.y * originalSize.y);
            contentRoot.anchoredPosition -= positionChange;
        }


        protected override void Render()
        {
            if (State.RenderObject is not RenderSliverList renderObject) return;

            if (rectMask != null) rectMask.enabled = State.UseMask;
            if (scrollRect.movementType != State.MovementType) scrollRect.movementType = State.MovementType;


            // var isHorizontal = State.Axis == Axis.Horizontal;
            // scrollRect.horizontal = isHorizontal;
            // scrollRect.vertical = !isHorizontal;
            //
            // EnsurePivotAndAnchorsAreConsistentWithDirection(isHorizontal);

            var isHorizontal = State.Axis == Axis.Horizontal;
            var axisChanged = scrollRect.horizontal != isHorizontal;

            scrollRect.horizontal = isHorizontal;
            scrollRect.vertical = !isHorizontal;

            if (axisChanged)
            {
                scrollRect.normalizedPosition = isHorizontal ? new Vector2(0, 0) : new Vector2(0, 1);


                var targetPivot = isHorizontal ? new Vector2(0, 0.5f) : new Vector2(0.5f, 1);
                var targetAnchorMin = isHorizontal ? new Vector2(0, 0) : new Vector2(0, 1);
                var targetAnchorMax = isHorizontal ? new Vector2(0, 1) : new Vector2(1, 1);

                contentRoot.anchorMin = targetAnchorMin;
                contentRoot.anchorMax = targetAnchorMax;
                contentRoot.pivot = targetPivot;


                // 3. Reset the content's anchored position AFTER setting the new anchors.
                //    This places the content correctly at the start of the reset ScrollRect.
                contentRoot.anchoredPosition = Vector2.zero;
                contentRoot.offsetMax = Vector2.zero;
                contentRoot.offsetMin = Vector2.zero;
            }

            var totalContentSize = isHorizontal
                ? renderObject.GetIntrinsicWidth(_rectTransform.rect.height)
                : renderObject.GetIntrinsicHeight(_rectTransform.rect.width);

            var currentSize = isHorizontal ? contentRoot.rect.width : contentRoot.rect.height;
            if (Mathf.Abs(currentSize - totalContentSize) > 0.01f)
                contentRoot.sizeDelta = isHorizontal
                    ? new Vector2(totalContentSize, contentRoot.sizeDelta.y)
                    : new Vector2(contentRoot.sizeDelta.x, totalContentSize);


            using (var render = _mapper.CreateRender())
            {
                var children = State.Children;
                var childrenLayout = renderObject.ChildrenLayout;
                for (var i = 0; i < children.Length; i++)
                {
                    var child = children[i];
                    var layoutData = childrenLayout[i];
                    var childView = render.RenderItem(child);
                    var rt = childView.rectTransform;
                    rt.anchorMin = new Vector2(0, 1);
                    rt.anchorMax = new Vector2(0, 1);
                    var pivotOffset = new Vector2(layoutData.Size.x * rt.pivot.x,
                        -layoutData.Size.y * (1.0f - rt.pivot.y));
                    rt.sizeDelta = layoutData.Size;
                    rt.anchoredPosition =
                        new Vector2(layoutData.CornerPosition.Value.x, -layoutData.CornerPosition.Value.y) +
                        pivotOffset;
                }
            }
        }

        private void OnScrollPositionChanged(Vector2 normalizedPosition)
        {
            if (_isUpdatingFromController || !HasState || State.StateLifetime.IsDisposed) return;

            var isHorizontal = State.Axis == Axis.Horizontal;
            var contentSize = contentRoot.rect.size;
            var viewportSize = _rectTransform.rect.size;

            var totalScrollableDist = isHorizontal ? contentSize.x - viewportSize.x : contentSize.y - viewportSize.y;
            if (totalScrollableDist < 0) totalScrollableDist = 0;

            var normalizedValue = isHorizontal ? normalizedPosition.x : 1 - normalizedPosition.y;

            State.ScrollOffset = normalizedValue * totalScrollableDist;
            State.ScrollController.NormalizedValue = normalizedValue;
        }

        public bool ScrollTo(int index, float duration, ScrollToPosition scrollToPosition, Easing easing)
        {
            if (State?.RenderObject is not RenderSliverList renderSliver) return false;

            var normalizedPosition = renderSliver.CalculateNormalizedOffset(index, scrollToPosition);

            // Stop any existing scroll animations before starting a new one.
            StopAllCoroutines();
            StartCoroutine(AnimateScrollTo(normalizedPosition, duration, easing));
            return true;
        }

        private IEnumerator AnimateScrollTo(float normalizedPosition, float duration,
            Easing easing)
        {
            var originalMovementType = scrollRect.movementType;
            try
            {
                scrollRect.movementType = ScrollRect.MovementType.Clamped;
                var time = 0f;
                var isHorizontal = State.Axis == Axis.Horizontal;

                var startPosition = isHorizontal
                    ? scrollRect.horizontalNormalizedPosition
                    : scrollRect.verticalNormalizedPosition;

                var targetPosition = isHorizontal
                    ? normalizedPosition
                    : 1f - normalizedPosition;

                while (time < duration)
                {
                    // Check for user interruption by comparing the current position with where
                    // our animation left it last frame. If they differ, the user has taken control.
                    var lastFrameValue = isHorizontal
                        ? scrollRect.horizontalNormalizedPosition
                        : scrollRect.verticalNormalizedPosition;
                    if (time > 0 && Mathf.Abs(lastFrameValue - startPosition) > 0.001f)
                        // Recalculate start position if interrupted, but for simplicity we can just break.
                        // A more advanced implementation could adjust the animation from the new start point.
                        yield break;

                    time += Time.unscaledDeltaTime;

                    var newPosition = Mathf.LerpUnclamped(
                        startPosition,
                        targetPosition,
                        easing(time, duration)
                    );

                    if (isHorizontal)
                        scrollRect.horizontalNormalizedPosition = newPosition;
                    else
                        scrollRect.verticalNormalizedPosition = newPosition;

                    startPosition = newPosition; // Update for next frame's interruption check.
                    yield return null;
                }

                // Ensure it ends at the exact target position.
                if (isHorizontal)
                    scrollRect.horizontalNormalizedPosition = targetPosition;
                else
                    scrollRect.verticalNormalizedPosition = targetPosition;
            }
            finally
            {
                scrollRect.movementType = originalMovementType;
            }
        }
    }


    internal interface IScrollingListState : IMultiChildLayoutState
    {
        ScrollController ScrollController { get; }
        Axis Axis { get; }

        public bool UseMask { get; }
        public ScrollRect.MovementType MovementType { get; }

        float ScrollOffset { get; set; }
        Vector2 ViewportSize { get; set; }
    }
}