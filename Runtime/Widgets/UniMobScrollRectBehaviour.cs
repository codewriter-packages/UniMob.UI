using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UniMob.UI.Widgets
{
    internal class UniMobScrollRectBehaviour : ScrollRect
    {
        private bool _routeToParent;
        private UniMobDismissibleDialogBehaviour _dismissibleDialog;

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData,
                ExecuteEvents.initializePotentialDrag);

            base.OnInitializePotentialDrag(eventData);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            _routeToParent =
                vertical && Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y) ||
                horizontal && Mathf.Abs(eventData.delta.x) < Mathf.Abs(eventData.delta.y);

            if (_routeToParent)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
                return;
            }

            _dismissibleDialog = GetComponentInParent<UniMobDismissibleDialogBehaviour>();

            if (_dismissibleDialog != null && _dismissibleDialog.HandleBeginDrag(eventData, this))
            {
                return;
            }

            base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (_routeToParent)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
                return;
            }

            if (_dismissibleDialog != null && _dismissibleDialog.HandleDrag(eventData))
            {
                return;
            }

            base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (_routeToParent)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
                return;
            }

            if (_dismissibleDialog != null && _dismissibleDialog.HandleEndDrag(eventData))
            {
                return;
            }

            base.OnEndDrag(eventData);
        }
    }
}