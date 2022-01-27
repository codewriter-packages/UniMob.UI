using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UniMob.UI.Widgets
{
    internal class UniMobScrollRectBehaviour : ScrollRect
    {
        private UniMobDismissibleDialogBehaviour _dismissibleDialog;

        public override void OnBeginDrag(PointerEventData eventData)
        {
            _dismissibleDialog = GetComponentInParent<UniMobDismissibleDialogBehaviour>();

            if (_dismissibleDialog != null && _dismissibleDialog.HandleBeginDrag(eventData, this))
            {
                return;
            }

            base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (_dismissibleDialog != null && _dismissibleDialog.HandleDrag(eventData))
            {
                return;
            }

            base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (_dismissibleDialog != null && _dismissibleDialog.HandleEndDrag(eventData))
            {
                return;
            }

            base.OnEndDrag(eventData);
        }
    }
}