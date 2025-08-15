using UnityEngine;

namespace UniMob.UI.Layout.Internal.RenderObjects
{
    /// <summary>
    /// A pure C# object that handles all layout calculation for a LayoutWidget.
    /// It is decoupled from MonoBehaviour and the Unity rendering pipeline.
    /// </summary>
    public abstract class RenderObject
    {
        public BuildContext Context { get; internal set; }
        public Vector2 Size { get; private set; } // The final size after layout

        /// <summary>
        /// Performs the layout calculation for this widget and its children.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        ///   <item>
        ///     <description>The RenderObject works exclusively in a logical, top-left coordinate system:</description>
        ///   </item>
        ///   <item>
        ///     <c>RenderObject.Size</c> (Vector2): The final, calculated width and height of the widget.
        ///   </item>
        ///   <item>
        ///     <c>RenderObject.ChildrenLayout[i].CornerPosition</c> (Vector2):
        ///     The <c>(x, y)</c> coordinate of the top-left corner of the i-th child's bounding box,
        ///     relative to the parent's top-left corner
        ///   </item>
        /// </list>
        /// </remarks>
        public void PerformLayout()
        {
            // Phase 1: Perform this widget's own size.
            this.Size = PerformSizing(this.Context.Constraints);

            // Phase 2: Perform layout for children.
            PerformPositioning();
        }

        /// <summary>
        /// Compute the size of the widget based on the provided layout constraints.
        /// This method is called during the layout pass to determine the size of the widget.
        /// </summary>
        /// <param name="contextConstraints"> The constraints imposed by the parent</param>
        /// <returns> A <see cref="Vector2"/> containing <c>(width, height)</c> </returns>
        protected abstract Vector2 PerformSizing(LayoutConstraints contextConstraints);

        /// <summary>
        /// Performs the positioning of the widget and its children.
        /// </summary>
        /// <remarks>
        /// This method is called after the size has been determined to position the widget.
        /// Subclasses should implement this to set the position of the widget, accessing, if necessary,
        /// the <see cref="Size"/> field that has been calculated in the sizing phase and <see cref="Context"/>
        /// to retrieve the layout constraints and other contextual information.
        /// <para>
        /// After this method is called, the positions of the children should be set in such a way that
        /// </para>
        ///  <para> In particular: 
        /// <list type="bullet">
        ///  <item>
        ///     Origin <c>(0,0)</c>: The top-left corner of the parent widget's available layout area.
        ///   </item>
        ///   <item>
        ///     X-Axis: Positive is to the right. Y-Axis: Positive is downwards.
        ///   </item>
        /// </list>
        /// </para>
        /// </remarks>
        protected abstract void PerformPositioning();

        /// <summary>
        /// A helper method to handle the boilerplate of laying out a child.
        /// It performs the layout and returns the child's final calculated size.
        /// </summary>
        /// <param name="child">The child state to layout.</param>
        /// <param name="constraints">The constraints to apply to the child.</param>
        /// <returns>The final size of the child after layout.</returns>

// Replace the entire LayoutChild method with this version.
        protected Vector2 LayoutChild(IState child, LayoutConstraints constraints)
        {
            if (child is null)
                return Vector2.zero;

            var childContext = new BuildContext(child, this.Context, constraints);

            if (child is ILayoutState childLayoutState && childLayoutState.RenderObject != null)
            {
                var childRenderObject = childLayoutState.RenderObject;
                childRenderObject.Context = childContext;
                childRenderObject.PerformLayout();
                return childRenderObject.Size;
            }

            // Legacy Widget Fallback:
            // The constraints from the parent are the absolute source of truth.
            // This correctly handles both fixed-size legacy widgets and those being
            // stretched by a parent (which will pass tight constraints).
            return child.Size.GetSize(new Vector2(constraints.MaxWidth, constraints.MaxHeight));
        }


        /// <summary>
        /// Calculates the widget's preferred width given an infinite height.
        /// </summary>
        public abstract float GetIntrinsicWidth(float height);

        /// <summary>
        /// Calculates the widget's preferred height given a specific width.
        /// </summary>
        public abstract float GetIntrinsicHeight(float width);
    }
}