using System;
using JetBrains.Annotations;
using UniMob.UI.Layout.Internal.RenderObjects;

namespace UniMob.UI
{
    public abstract class StatefulWidget : Widget
    {
        private Type _type;

        public Type Type => _type ?? (_type = GetType());

        [CanBeNull] public Key Key { get; set; }

        [NotNull]
        public virtual State CreateState(StateProvider provider)
        {
            return provider.Of(this);
        }

        [CanBeNull]
        public virtual State CreateState()
        {
            return null;
        }

        /// <summary>
        /// Creates the lightweight RenderObject responsible for layout calculations.
        /// </summary>
        public virtual RenderObject CreateRenderObject(BuildContext context, IState state)
        {
            if (state is IViewState viewState)
            {
                return new RenderLegacy(viewState);
            }

            return state.InnerViewState.RenderObject;
        }
    }
}