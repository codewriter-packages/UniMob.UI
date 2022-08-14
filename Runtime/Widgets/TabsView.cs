using System;
using UniMob.UI.Internal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UniMob.UI.Widgets
{
    public class TabsView : View<ITabsState>, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform content = default;
        [SerializeField] private RectMask2D rectMask = default;

        private bool _fastSwipeTimer;
        private float _fastSwipeTime;
        private Vector2 _positionOnDragStart;

        private ViewMapperBase _mapper;
        private Canvas _canvas;

        private bool _routeToParent;

        protected override void Activate()
        {
            base.Activate();

            if (_mapper == null)
                _mapper = new PooledViewMapper(content);
        }

        protected override void Render()
        {
            var useMask = State.UseMask;
            if (rectMask.enabled != useMask)
            {
                rectMask.enabled = useMask;
            }

            foreach (var child in State.Children)
            {
                if (!float.IsInfinity(child.Size.MaxHeight) || !float.IsInfinity(child.Size.MaxWidth))
                {
                    Debug.LogError("Cannot render fixed-size widgets inside Tabs.\n" +
                                   $"Try to wrap '{child.GetType().Name}' into another stretched widget");
                }
            }

            var children = State.Children;
            var bounds = Bounds;
            var tabSize = (Vector2) bounds;
            var tabValue = State.TabController.Value;
            var tabCount = State.TabController.TabCount;
            var firstTab = Convert.ToInt32(Math.Floor(tabValue));
            var secondTab = Convert.ToInt32(Math.Ceiling(tabValue));

            if (tabCount != children.Length)
            {
                Debug.LogError($"TabController.TabCount != Children.Length");
                return;
            }

            using (var render = _mapper.CreateRender())
            {
                for (var index = 0; index < children.Length; index++)
                {
                    if (index != firstTab && index != secondTab)
                    {
                        render.Reuse(children[index]);
                    }
                }

                var cornerPos = new Vector2(-tabSize.x * (tabValue - firstTab), 0f);

                if (firstTab == secondTab)
                {
                    RenderChild(render, children[firstTab], tabSize, cornerPos);
                }
                else
                {
                    RenderChild(render, children[firstTab], tabSize, cornerPos);
                    RenderChild(render, children[secondTab], tabSize, cornerPos + new Vector2(tabSize.x, 0f));
                }
            }
        }

        private void RenderChild(ViewMapperBase.ViewMapperRenderScope render,
            IState child, Vector2 size, Vector2 cornerPosition)
        {
            var childView = render.RenderItem(child);

            LayoutData layout;
            layout.Size = size;
            layout.Alignment = Alignment.TopLeft;
            layout.Corner = Alignment.TopLeft;
            layout.CornerPosition = cornerPosition;
            ViewLayoutUtility.SetLayout(childView.rectTransform, layout);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _routeToParent = Mathf.Abs(eventData.delta.x) < Mathf.Abs(eventData.delta.y);

            if (_routeToParent)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
                return;
            }

            _canvas = GetComponentInParent<Canvas>();
            _fastSwipeTimer = true;
            _fastSwipeTime = 0;
            _positionOnDragStart = eventData.position;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (_routeToParent)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
                return;
            }

            if (!HasState)
            {
                return;
            }

            using (Atom.NoWatch)
            {
                var delta = eventData.delta.x / _canvas.scaleFactor / Bounds.x;
                State.TabController.SetValue(State.TabController.Value - delta);
            }

            if (_fastSwipeTimer)
            {
                _fastSwipeTime += Time.unscaledDeltaTime;
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (_routeToParent)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
                return;
            }

            if (!HasState)
            {
                return;
            }

            _fastSwipeTimer = false;

            using (Atom.NoWatch)
            {
                var change = _positionOnDragStart.x - eventData.position.x;
                var isFastSwipe = _fastSwipeTime < 0.2f && Mathf.Abs(change) > 100;

                var prevTab = State.TabController.Value;
                var nextTab = isFastSwipe && change < 0 ? Math.Floor(prevTab)
                    : isFastSwipe && change > 0 ? Math.Ceiling(prevTab)
                    : Math.Round(prevTab);

                State.TabController.AnimateTo(Convert.ToInt32(nextTab));
            }
        }
    }

    public interface ITabsState : IViewState
    {
        TabController TabController { get; }

        bool UseMask { get; }

        IState[] Children { get; }
    }
}