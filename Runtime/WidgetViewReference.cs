using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UniMob.UI
{
    public struct WidgetViewReference : IEquatable<WidgetViewReference>
    {
        private WidgetViewReference(WidgetViewReferenceType type, string path, AssetReferenceGameObject reference,
            GameObject prefab)
        {
            Type = type;
            Path = path;
            Reference = reference;
            Prefab = prefab;
        }

        public WidgetViewReferenceType Type { get; }
        public string Path { get; }
        public AssetReferenceGameObject Reference { get; }
        public GameObject Prefab { get; }

        public bool Equals(WidgetViewReference other)
        {
            return Type == other.Type && Path == other.Path && Reference == other.Reference && Prefab == other.Prefab;
        }

        public override bool Equals(object obj)
        {
            return obj is WidgetViewReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return unchecked((int) Type * 397) ^ (Path != null ? Path.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return $"[{nameof(WidgetViewReference)}: {Type} {Path} {Reference} {Prefab}]";
        }

        public static WidgetViewReference Addressable(string path)
        {
            return new WidgetViewReference(WidgetViewReferenceType.Addressable, path, null, null);
        }

        public static WidgetViewReference Addressable(AssetReferenceGameObject reference)
        {
            return new WidgetViewReference(WidgetViewReferenceType.Addressable, null, reference, null);
        }

        public static WidgetViewReference Resource(string path)
        {
            return new WidgetViewReference(WidgetViewReferenceType.Resource, path, null, null);
        }

        public static WidgetViewReference FromPrefab(GameObject prefab)
        {
            return new WidgetViewReference(WidgetViewReferenceType.Prefab, null, null, prefab);
        }

        public static implicit operator WidgetViewReference(AssetReferenceGameObject reference)
        {
            return Addressable(reference);
        }
    }

    public enum WidgetViewReferenceType
    {
        Resource,
        Addressable,
        Prefab,
    }
}