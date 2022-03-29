using System;

namespace UniMob.UI
{
    public sealed class ViewRenderScope
    {
        private readonly Scope _scope = new Scope();

        public void Link(IViewTreeElement self)
        {
            _scope.Link(self);
        }

        public IDisposable Enter(IViewTreeElement self)
        {
            _scope.Enter(self);
            return _scope;
        }

        private class Scope : IDisposable
        {
            private IViewTreeElement _prevElement;

            public void Link(IViewTreeElement self)
            {
                if (UniMobViewContext.CurrentElement != null)
                    UniMobViewContext.CurrentElement.AddChild(self);
            }

            public void Enter(IViewTreeElement self)
            {
                _prevElement = UniMobViewContext.CurrentElement;
                UniMobViewContext.CurrentElement = self;
            }

            public void Dispose()
            {
                UniMobViewContext.CurrentElement = _prevElement;
            }
        }
    }
}