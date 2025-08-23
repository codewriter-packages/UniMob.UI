using UniMob.UI.Layout.Internal.RenderObjects;
using UnityEngine;

namespace UniMob.UI.Layout
{
    // Interface for easier type-checking later without generics.
    public interface ILayoutState : IViewState
    {
    }

    /// <summary>
    /// The base State for a LayoutWidget. It creates and owns the RenderObject.
    /// </summary>
    public abstract class LayoutState<TWidget> : ViewState<TWidget>, ILayoutState where TWidget : LayoutWidget, Widget
    {
    }
}