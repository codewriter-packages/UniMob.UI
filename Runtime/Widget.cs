using System;
using UniMob.UI.Layout.Internal.RenderObjects;

namespace UniMob.UI
{
    // ReSharper disable once InconsistentNaming
    public interface Widget
    {
        Type Type { get; }

        Key Key { get; }

        State CreateState(StateProvider provider);
        State CreateState();

        /// <summary>
        /// Creates the lightweight RenderObject responsible for layout calculations.
        /// </summary>
        RenderObject CreateRenderObject(BuildContext context, IState state);
    }
}