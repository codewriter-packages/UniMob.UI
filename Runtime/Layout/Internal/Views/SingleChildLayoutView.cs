using UniMob.UI;
using UniMob.UI.Internal;
using UnityEngine;



[assembly: RegisterComponentViewFactory("$$_Layout.SingleChildLayoutView",
    typeof(UniMob.UI.Layout.Internal.Views.SingleChildLayoutView))]

namespace UniMob.UI.Layout.Internal.Views
{
    /// <summary>
    /// Represents a layout view that manages a single child element within a UI hierarchy.
    /// </summary>
    /// <remarks>This class requires the associated GameObject to have both a <see cref="RectTransform"/> and
    /// a <see cref="CanvasRenderer"/> component. It provides a base implementation for managing the layout of a single
    /// child element, typically used in custom UI components.</remarks>
    [RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
    public class SingleChildLayoutView : SingleChildLayoutView<ISingleChildLayoutState>
    {

    }
}