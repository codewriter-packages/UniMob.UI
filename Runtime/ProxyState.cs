using System;

namespace UniMob.UI
{
    public abstract class ProxyWidget : StatefulWidget
    {
        private readonly Func<Widget> _childBuilder;
        private readonly Widget _child;

        protected ProxyWidget(Func<Widget> childBuilder) => _childBuilder = childBuilder;
        protected ProxyWidget(Widget child) => _child = child;

        public Widget Child => _childBuilder?.Invoke() ?? _child;
    }

    public abstract class ProxyState<TWidget> : HocState<TWidget>
        where TWidget : ProxyWidget
    {
        public sealed override Widget Build(BuildContext context) => Widget.Child;
    }
}