using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    /// <summary>
    /// Represents a render object that manages a single child and provides layout and rendering behavior for it.
    /// </summary>
    /// <remarks>This abstract class serves as a base for render objects that are responsible for managing a
    /// single child. It provides properties to access the child's size and position, as well as methods to calculate
    /// intrinsic dimensions based on the child's layout. Subclasses are expected to define specific layout and
    /// rendering behavior for the child.</remarks>
    public abstract class SingleChildRenderObject : RenderObject, ISingleChildRenderObject
    {
        private readonly ISingleChildLayoutState _state;
        public Vector2 ChildSize { get; protected set; }
        public Vector2 ChildPosition { get; protected set; }

        protected IState Child => _state.Child;

        protected SingleChildRenderObject(ISingleChildLayoutState state)
        {
            _state = state;
        }


        public override float GetIntrinsicWidth(float height)
        {
            if (Child != null)
            {
                return Child.RenderObject.GetIntrinsicWidth(height);
            }
            return 0;
        }

        public override float GetIntrinsicHeight(float width)
        {
            if (Child != null)
            {
                return Child.RenderObject.GetIntrinsicHeight(width);
            }
            return 0;
        }
    }
}