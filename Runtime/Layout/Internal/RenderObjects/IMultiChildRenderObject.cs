using System.Collections.Generic;
using UniMob.UI.Widgets;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    public interface IMultiChildRenderObject
    {
        IReadOnlyList<LayoutData> ChildrenLayout { get; }
    }
}