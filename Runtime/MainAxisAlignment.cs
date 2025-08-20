namespace UniMob.UI
{
    public enum MainAxisAlignment
    {
        /// <summary>
        /// Place the children as close to the start of the main axis as possible.
        /// </summary>
        Start = 0,

        /// <summary>
        /// Place the children as close to the end of the main axis as possible.
        /// </summary>
        End = 1,

        /// <summary>
        /// Place the children as close to the middle of the main axis as possible.
        /// </summary>
        Center = 2,
        
        /// <summary>
        /// Place the free space evenly between the children.
        /// </summary>
        /// <remarks>Only supported in Layout widgets.</remarks>
        SpaceBetween = 3,

        /// <summary>
        /// Place the free space evenly between the children as well as half
        /// of that space before and after the first and last child.
        /// </summary>
        /// <remarks>Only supported in Layout widgets.</remarks>
        SpaceAround = 4,

        /// <summary>
        /// Place the free space evenly between the children as well as before and after the first and last child.
        /// </summary>
        /// <remarks>Only supported in Layout widgets.</remarks>
        SpaceEvenly = 5,
    }
}