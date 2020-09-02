using System;
using UnityEngine;

namespace UniMob.UI
{
    [Serializable]
    public class FloatTween : IAnimatable<float>
    {
        [SerializeField] private float begin;
        [SerializeField] private float end;

        private FloatTween()
        {
        }

        public FloatTween(float begin, float end)
        {
            this.begin = begin;
            this.end = end;
        }

        public float Transform(float t) => begin + (end - begin) * t;
    }

    [Serializable]
    public class Vector2Tween : IAnimatable<Vector2>
    {
        [SerializeField] private Vector2 begin;
        [SerializeField] private Vector2 end;

        private Vector2Tween()
        {
        }

        public Vector2Tween(Vector2 begin, Vector2 end)
        {
            this.begin = begin;
            this.end = end;
        }

        public Vector2 Transform(float t) => new Vector2(
            begin.x + (end.x - begin.x) * t,
            begin.y + (end.y - begin.y) * t
        );
    }

    [Serializable]
    public class Vector3Tween : IAnimatable<Vector3>
    {
        [SerializeField] private Vector3 begin;
        [SerializeField] private Vector3 end;

        private Vector3Tween()
        {
        }

        public Vector3Tween(Vector3 begin, Vector3 end)
        {
            this.begin = begin;
            this.end = end;
        }

        public Vector3 Transform(float t) => new Vector3(
            begin.x + (end.x - begin.x) * t,
            begin.y + (end.y - begin.y) * t,
            begin.z + (end.z - begin.z) * t
        );
    }

    [Serializable]
    public class QuaternionTween : IAnimatable<Quaternion>
    {
        [SerializeField] private Quaternion begin;
        [SerializeField] private Quaternion end;

        private QuaternionTween()
        {
        }

        public QuaternionTween(Quaternion begin, Quaternion end)
        {
            this.begin = begin;
            this.end = end;
        }

        public Quaternion Transform(float t) => Quaternion.LerpUnclamped(begin, end, t);
    }
}