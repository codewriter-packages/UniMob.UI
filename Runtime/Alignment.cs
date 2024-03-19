using UnityEngine;

namespace UniMob.UI
{
    public struct Alignment
    {
        public float X { get; }
        public float Y { get; }

        public Alignment(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Alignment WithTop() => new Alignment(X, TopCenter.Y);

        public Alignment WithCenterY() => new Alignment(X, Center.Y);
        
        public Alignment WithLeft() => new Alignment(CenterLeft.X, Y);
        public Alignment WithRight() => new Alignment(CenterRight.X, Y);
        
        public Alignment WithCenterX() => new Alignment(Center.X, Y);

        public Vector2 ToAnchor() => new Vector2(X * 0.5f + 0.5f, -Y * 0.5f + 0.5f);

        /// <summary>
        /// The center point along the bottom edge.
        /// </summary>
        public static readonly Alignment BottomCenter = new Alignment(0.0f, 1.0f);

        /// <summary>
        ///  The bottom left corner.
        /// </summary>
        public static readonly Alignment BottomLeft = new Alignment(-1.0f, 1.0f);

        /// <summary>
        /// The bottom right corner.
        /// </summary>
        public static readonly Alignment BottomRight = new Alignment(1.0f, 1.0f);

        /// <summary>
        /// The center point, both horizontally and vertically.
        /// </summary>
        public static readonly Alignment Center = new Alignment(0.0f, 0.0f);

        /// <summary>
        /// The center point along the left edge.
        /// </summary>
        public static readonly Alignment CenterLeft = new Alignment(-1.0f, 0.0f);

        /// <summary>
        /// The center point along the right edge.
        /// </summary>
        public static readonly Alignment CenterRight = new Alignment(1.0f, 0.0f);

        /// <summary>
        /// The center point along the top edge.
        /// </summary>
        public static readonly Alignment TopCenter = new Alignment(0.0f, -1.0f);

        /// <summary>
        /// The top left corner.
        /// </summary>
        public static readonly Alignment TopLeft = new Alignment(-1.0f, -1.0f);

        /// <summary>
        /// The top right corner.
        /// </summary>
        public static readonly Alignment TopRight = new Alignment(1.0f, -1.0f);
    }
}