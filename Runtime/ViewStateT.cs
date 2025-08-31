using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace UniMob.UI
{
    public abstract class ViewState<TWidget> : ViewState
        where TWidget : Widget
    {
        private Dictionary<string, StateHolder> _renderChildCache;
        
        private readonly MutableAtom<TWidget> _widget = Atom.Value(default(TWidget));

        protected TWidget Widget => _widget.Value;

        internal sealed override void Update(Widget widget)
        {
            base.Update(widget);

            var oldWidget = Widget;

            if (widget is TWidget typedWidget)
            {
                _widget.Value = typedWidget;
            }
            else
            {
                throw new WrongStateTypeException(GetType(), typeof(TWidget), widget.GetType());
            }

            if (oldWidget != null)
            {
                DidUpdateWidget(oldWidget);
            }
        }

        public virtual void DidUpdateWidget([NotNull] TWidget oldWidget)
        {
            Assert.IsNull(Atom.CurrentScope);
        }

        protected IState RenderChild(WidgetBuilder<Widget> builder, [CallerMemberName] string cacheKey = "")
        {
            return CreateChildCached(cacheKey, builder).Value;
        }

        [MustUseReturnValue]
        protected RenderChildBuilder<TChildWidget> RenderChildT<TChildWidget>(WidgetBuilder<TChildWidget> builder,
            [CallerMemberName] string cacheKey = "")
            where TChildWidget : Widget
        {
            return new RenderChildBuilder<TChildWidget>(this, cacheKey, builder);
        }

        protected readonly ref struct RenderChildBuilder<TChildWidget> where TChildWidget : Widget
        {
            private readonly ViewState<TWidget> _owner;
            private readonly string _cacheKey;
            private readonly WidgetBuilder<TChildWidget> _builder;

            public RenderChildBuilder(ViewState<TWidget> owner, string cacheKey, WidgetBuilder<TChildWidget> builder)
            {
                (_owner, _cacheKey, _builder) = (owner, cacheKey, builder);
            }

            public TChildState As<TChildState>() where TChildState : ViewState<TChildWidget>
            {
                var state = _owner.CreateChildCached(_cacheKey, _builder).Value;
                if (state is TChildState typedState)
                {
                    return typedState;
                }

                throw new InvalidOperationException(
                    $"RenderChild failed, cannot cast '{state?.GetType().FullName}' to '{typeof(TChildState).FullName}'");
            }
        }

        private StateHolder CreateChildCached<TChildWidget>(string cacheKey, WidgetBuilder<TChildWidget> builder)
            where TChildWidget : Widget
        {
            _renderChildCache ??= new Dictionary<string, StateHolder>();
            
            if (!_renderChildCache.TryGetValue(cacheKey, out var cachedStateHolder))
            {
                _renderChildCache[cacheKey] = cachedStateHolder =
                    Create<TChildWidget, IState>(StateLifetime, new BuildContext(this, Context), builder);
            }

            return cachedStateHolder;
        }

        protected StateHolder<TChildState> CreateChild<TChildWidget, TChildState>(WidgetBuilder<TChildWidget> builder)
            where TChildWidget : Widget
            where TChildState : ViewState<TChildWidget>
        {
            return Create<TChildWidget, TChildState>(StateLifetime, new BuildContext(this, Context), builder);
        }

        protected StateHolder CreateChild(WidgetBuilder<Widget> builder)
        {
            return Create<Widget, IState>(StateLifetime, new BuildContext(this, Context), builder);
        }

        protected StateCollectionHolder CreateChildren(Func<BuildContext, List<Widget>> builder)
        {
            return CreateList(StateLifetime, new BuildContext(this, Context), builder);
        }
    }
}