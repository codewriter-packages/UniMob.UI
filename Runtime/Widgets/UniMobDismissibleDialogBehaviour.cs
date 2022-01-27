using UnityEngine;
using UnityEngine.EventSystems;

namespace UniMob.UI.Widgets
{
    internal class UniMobDismissibleDialogBehaviour : UIBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [SerializeField] private float offsetLerpStrength = 10;

        private bool _dragging;
        private float _dragLastDelta;
        private UniMobScrollRectBehaviour _dragScrollRect;

        internal IDismissibleDialogState State { get; set; }

        private RectTransform Rect => (RectTransform) transform;

        private void Update()
        {
            if (State == null)
            {
                return;
            }

            if (!_dragging && Mathf.Abs(State.Offset) != 0f)
            {
                var offset = Mathf.Lerp(State.Offset, 0f, Time.smoothDeltaTime * offsetLerpStrength);
                if (Mathf.Abs(offset) < 1f)
                {
                    offset = 0f;
                }

                State.SetOffset(offset);
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            HandleBeginDrag(eventData, this);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            HandleDrag(eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            HandleEndDrag(eventData);
        }

        internal bool HandleBeginDrag(PointerEventData eventData, Component hitComponent)
        {
            if (eventData.button == PointerEventData.InputButton.Left && IsActive() && State != null)
            {
                _dragging = true;
                _dragScrollRect = hitComponent as UniMobScrollRectBehaviour;
            }

            return false;
        }

        internal bool HandleDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && IsActive() && _dragging && State != null)
            {
                _dragLastDelta = eventData.delta.y * -1f;

                // _dragLastDelta < 0f is scroll up
                if (_dragScrollRect == null ||
                    State.Expanded == false ||
                    State.Expanded && State.Offset > 0 ||
                    State.Expanded && _dragLastDelta > 0f && _dragScrollRect.verticalNormalizedPosition >= 1f)
                {
                    State.SetOffset(State.Offset + _dragLastDelta);
                    return true;
                }
            }

            return false;
        }

        internal bool HandleEndDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && IsActive() && _dragging && State != null)
            {
                var overThreshold = Mathf.Abs(State.Offset) > Rect.rect.size.y * State.DismissThreshold;
                if (overThreshold)
                {
                    if (_dragLastDelta < 0f && State.Offset < 0f && !State.Expanded)
                    {
                        State.OnExpand();
                    }
                    else if (_dragLastDelta > 0f && State.Offset > 0f && State.Expanded)
                    {
                        State.OnCollapse();
                    }
                    else if (_dragLastDelta > 0f && State.Offset > 0f && !State.Expanded)
                    {
                        State.OnDismiss();
                    }
                }
            }

            _dragging = false;
            _dragScrollRect = null;

            return false;
        }
    }
}