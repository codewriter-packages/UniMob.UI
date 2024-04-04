using System;
using UnityEngine;

namespace UniMob.UI
{
    [Serializable]
    public struct RectPadding
    {
        [SerializeField] private float left;
        [SerializeField] private float right;
        [SerializeField] private float top;
        [SerializeField] private float bottom;

        public RectPadding(float left, float right, float top, float bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        public float Left => left;
        public float Right => right;
        public float Top => top;
        public float Bottom => bottom;

        public float Horizontal => Left + Right;
        public float Vertical => Top + Bottom;

        public Vector2 Center => new Vector2(Left - Right, Top - Bottom);

        internal float GetLeft(int axis) => axis == 0 ? Left : Top;
        internal float GetRight(int axis) => axis == 0 ? Right : Bottom;
        internal float GetTop(int axis) => axis == 0 ? Top : Left;
        internal float GetBottom(int axis) => axis == 0 ? Bottom : Right;
    }
}