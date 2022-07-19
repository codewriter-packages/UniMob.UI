using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace UniMob.UI.Internal
{
    public abstract class ViewMapperBase : IViewTreeElement
    {
        private readonly ViewMapperRenderScope _mapperScope;
        private readonly ViewRenderScope _renderScope;
        private readonly bool _link;

        private readonly List<Item> _reuse = new List<Item>();
        private List<Item> _items = new List<Item>();
        private List<Item> _next = new List<Item>();

        private IDisposable _activeRender;

        private class Item
        {
            public WidgetViewReference ViewReference;
            public IView View;
            public IState State;
        }

        protected ViewMapperBase(bool link)
        {
            _link = link;
            _mapperScope = new ViewMapperRenderScope(this);
            _renderScope = new ViewRenderScope();
        }

        protected abstract IView ResolveView(WidgetViewReference state);
        protected abstract void RecycleView(IView view);

        void IViewTreeElement.AddChild(IViewTreeElement view)
        {
        }

        void IViewTreeElement.Unmount()
        {
            RecycleItemsAndClear(_items);
            RecycleItemsAndClear(_reuse);
            Assert.AreEqual(0, _next.Count);
        }

        public ViewMapperRenderScope CreateRender()
        {
            BeginRender();
            return _mapperScope;
        }

        private void BeginRender()
        {
            if (_activeRender != null)
            {
                throw new InvalidOperationException("Must not call Render() inside other Render()");
            }

            _renderScope.Link(this);
            _activeRender = _renderScope.Enter(this);

            Assert.AreEqual(0, _next.Count);
        }

        private void EndRender()
        {
            if (_activeRender == null)
            {
                throw new InvalidOperationException("Must not call EndRender() without BeginRender()");
            }

            RecycleItemsAndClear(_items);

            var old = _items;
            _items = _next;
            _next = old;

            foreach (var reusableItem in _reuse)
            {
                reusableItem.View.gameObject.SetActive(false);
            }

            _activeRender?.Dispose();
            _activeRender = null;
            
            foreach (var item in _items)
            {
                item.View.SetSource(item.State.InnerViewState, _link);
            }
        }

        private bool Reuse(IState state)
        {
            if (!TryFindActiveItemIndex(state, out var itemIndex))
            {
                return false;
            }

            var item = _items[itemIndex];
            item.View.ResetSource();

            _reuse.Add(item);

            _items[itemIndex] = _items[_items.Count - 1];
            _items.RemoveAt(_items.Count - 1);

            return true;
        }

        private IView RenderItem(IState state)
        {
            var viewState = state.InnerViewState;
            var nextViewReference = viewState.View;

            Item item;
            if (TryFindActiveItemIndex(state, out var itemIndex))
            {
                item = _items[itemIndex];

                _items[itemIndex] = _items[_items.Count - 1];
                _items.RemoveAt(_items.Count - 1);

                if (!item.ViewReference.Equals(nextViewReference))
                {
                    item.View.ResetSource();
                    RecycleView(item.View);
                    item.View = null;
                }
            }
            else
            {
                item = new Item {State = state};
            }

            if (item.View == null)
            {
                item.View = ResolveOrReuseView(nextViewReference);
            }

            item.ViewReference = nextViewReference;

            _next.Add(item);

            return item.View;
        }

        private IView ResolveOrReuseView(WidgetViewReference viewReference)
        {
            for (var i = 0; i < _reuse.Count; i++)
            {
                var item = _reuse[i];
                if (!item.ViewReference.Equals(viewReference))
                {
                    continue;
                }

                _reuse.RemoveAt(i);

                var view = item.View;

                if (!view.gameObject.activeSelf)
                {
                    view.gameObject.SetActive(true);
                }

                return view;
            }

            return ResolveView(viewReference);
        }

        private bool TryFindActiveItemIndex(IState state, out int itemIndex)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (!ReferenceEquals(_items[i].State, state))
                {
                    continue;
                }

                itemIndex = i;
                return true;
            }

            itemIndex = default;
            return false;
        }

        private void RecycleItemsAndClear(List<Item> list)
        {
            if (list.Count == 0)
            {
                return;
            }

            for (var index = 0; index < list.Count; index++)
            {
                var removed = list[index];
                removed.View.ResetSource();
                RecycleView(removed.View);
            }

            list.Clear();
        }

        public class ViewMapperRenderScope : IDisposable
        {
            private readonly ViewMapperBase _mapper;

            public ViewMapperRenderScope(ViewMapperBase mapper)
            {
                _mapper = mapper;
            }

            void IDisposable.Dispose()
            {
                _mapper.EndRender();
            }

            public IView RenderItem(IState state)
            {
                return _mapper.RenderItem(state);
            }

            public bool Reuse(IState state)
            {
                return _mapper.Reuse(state);
            }
        }
    }
}