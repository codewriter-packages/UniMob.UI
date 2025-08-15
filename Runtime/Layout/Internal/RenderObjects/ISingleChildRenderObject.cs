using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    public interface ISingleChildRenderObject
    {
        Vector2 ChildSize { get; }
        Vector2 ChildPosition { get; }
    }
}