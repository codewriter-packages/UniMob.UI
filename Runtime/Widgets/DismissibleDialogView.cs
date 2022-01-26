using System.Collections.Generic;
using UniMob.UI.Internal;
using UniMob.UI.Widgets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[assembly: RegisterComponentViewFactory("$$_DismissibleDialog",
    typeof(RectTransform), typeof(DismissibleDialogView))]

namespace UniMob.UI.Widgets
{
    internal sealed class DismissibleDialogView : View<IDismissibleDialogState>
    {
        private ViewMapperBase _mapper;
        private RectTransform _dismissibleDialogTransform;
        private DismissibleDialogGraphic _dismissibleDialogGraphic;

        protected override void Activate()
        {
            if (_dismissibleDialogGraphic == null)
            {
                var go = new GameObject("DismissibleDialogGraphic",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(DismissibleDialogGraphic));
                _dismissibleDialogTransform = (RectTransform) go.transform;
                _dismissibleDialogTransform.SetParent(transform, false);
                _dismissibleDialogGraphic = go.GetComponent<DismissibleDialogGraphic>();
            }

            if (_mapper == null)
                _mapper = new PooledViewMapper(transform);

            _dismissibleDialogGraphic.State = State;

            LayoutData layout;
            layout.Size = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            layout.Alignment = Alignment.Center;
            layout.Corner = Alignment.Center;
            layout.CornerPosition = Vector2.zero;
            ViewLayoutUtility.SetLayout(_dismissibleDialogTransform, layout);

            base.Activate();

            Atom.Reaction(StateLifetime,
                () => Bounds.y,
                v => State.SetExpandedHeight(v),
                fireImmediately: true);
        }

        protected override void Deactivate()
        {
            _dismissibleDialogGraphic.State = null;

            base.Deactivate();
        }

        protected override void Render()
        {
            using (var render = _mapper.CreateRender())
            {
                var child = State.Child;
                var childView = render.RenderItem(child);
                var childSize = State.Child.Size.GetSize(State.ChildSize.GetSizeUnbounded());

                LayoutData layout;
                layout.Size = childSize;
                layout.Alignment = Alignment.BottomCenter;
                layout.Corner = Alignment.BottomCenter;
                layout.CornerPosition = Vector2.zero;
                ViewLayoutUtility.SetLayout(childView.rectTransform, layout);
            }

            _dismissibleDialogGraphic.transform.SetAsLastSibling();
        }
    }

    internal class DismissibleDialogGraphic : Graphic,
        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler,
        IPointerClickHandler,
        IPointerDownHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerUpHandler,
        IScrollHandler
    {
        private readonly List<RaycastResult> _graphicRaycastResult = new List<RaycastResult>();

        private bool _dragging;
        private float _dragLastDelta;
        private ScrollRect _dragScrollRect;
        private GameObject _pressHandler;

        public IDismissibleDialogState State { get; set; }

        private RectTransform Rect => (RectTransform) transform;

        public override void SetMaterialDirty()
        {
        }

        public override void SetVerticesDirty()
        {
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }

        private void Update()
        {
            if (State == null)
            {
                return;
            }

            if (!_dragging && Mathf.Abs(State.Offset) != 0f)
            {
                var offset = Mathf.Lerp(State.Offset, 0f, Time.smoothDeltaTime * 10f);
                if (Mathf.Abs(offset) < 1f)
                {
                    offset = 0f;
                }

                State.SetOffset(offset);
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (_pressHandler != null)
            {
                ExecuteEvents.Execute(_pressHandler, eventData, ExecuteEvents.pointerUpHandler);

                _pressHandler = null;
            }

            if (eventData.button == PointerEventData.InputButton.Left && IsActive() && State != null)
            {
                _dragging = true;

                var target = PropagateEvent(eventData, ExecuteEvents.beginDragHandler);

                if (target != null &&
                    target.TryGetComponent(out ScrollRect scrollRect) &&
                    scrollRect.vertical)
                {
                    _dragScrollRect = scrollRect;
                }
            }
            else
            {
                PropagateEvent(eventData, ExecuteEvents.beginDragHandler);
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
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
                }
                else
                {
                    PropagateEvent(eventData, ExecuteEvents.dragHandler);
                }
            }
            else
            {
                PropagateEvent(eventData, ExecuteEvents.dragHandler);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && IsActive() && _dragging && State != null)
            {
                var target = PropagateEvent(eventData, ExecuteEvents.endDragHandler);

                if (_dragScrollRect != null && target != _dragScrollRect.gameObject)
                {
                    ExecuteEvents.Execute(_dragScrollRect.gameObject, eventData, ExecuteEvents.endDragHandler);
                }

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
            else
            {
                PropagateEvent(eventData, ExecuteEvents.endDragHandler);
            }

            _dragging = false;
            _dragScrollRect = null;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (_dragging)
            {
                return;
            }

            PropagateEvent(eventData, ExecuteEvents.pointerClickHandler);
        }

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            PropagateEvent(eventData, ExecuteEvents.initializePotentialDrag);
        }

        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            PropagateEvent(eventData, ExecuteEvents.dropHandler);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            _pressHandler = PropagateEvent(eventData, ExecuteEvents.pointerDownHandler);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            PropagateEvent(eventData, ExecuteEvents.pointerEnterHandler);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            PropagateEvent(eventData, ExecuteEvents.pointerExitHandler);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (_pressHandler == null)
            {
                return;
            }

            PropagateEvent(eventData, ExecuteEvents.pointerUpHandler);

            _pressHandler = null;
        }

        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            PropagateEvent(eventData, ExecuteEvents.scrollHandler);
        }

        private GameObject PropagateEvent<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> callback)
            where T : IEventSystemHandler
        {
            GameObject target = null;

            if (!raycastTarget)
            {
                Debug.LogError("DismissibleDialogGraphic.raycastTarget == false");
                return null;
            }

            raycastTarget = false;

            EventSystem.current.RaycastAll(eventData, _graphicRaycastResult);
            var root = FindFirstRaycast(_graphicRaycastResult).gameObject;
            _graphicRaycastResult.Clear();

            if (root != null && root.gameObject != gameObject)
            {
                target = ExecuteEvents.ExecuteHierarchy(root.gameObject, eventData, callback);
            }

            raycastTarget = true;

            return target;
        }

        protected static RaycastResult FindFirstRaycast(List<RaycastResult> candidates)
        {
            for (var i = 0; i < candidates.Count; ++i)
            {
                if (candidates[i].gameObject == null)
                    continue;

                return candidates[i];
            }

            return new RaycastResult();
        }
    }

    internal interface IDismissibleDialogState : IViewState
    {
        float Offset { get; }
        float DismissThreshold { get; }
        bool Expanded { get; }
        bool Dismissed { get; }
        IState Child { get; }
        WidgetSize ChildSize { get; }

        void SetOffset(float offset);
        void SetExpandedHeight(float height);
        void OnExpand();
        void OnCollapse();
        void OnDismiss();
    }
}