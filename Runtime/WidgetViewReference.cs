using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UniMob.UI
{
    public struct WidgetViewReference : IEquatable<WidgetViewReference>
    {
        private Atom<WidgetViewReference> _source;
        private WidgetViewReferenceType _type;
        private string _path;
        private AssetReferenceGameObject _reference;
        private GameObject _prefab;

        private WidgetViewReference(WidgetViewReferenceType type, string path, AssetReferenceGameObject reference, GameObject prefab)
        {
            _type = type;
            _path = path;
            _reference = reference;
            _prefab = prefab;
            _source = null;
        }

        internal WidgetViewReference(Atom<WidgetViewReference> source)
        {
            _type = WidgetViewReferenceType.Resource;
            _path = null;
            _reference = null;
            _prefab = null;
            _source = source;
        }

        internal void LinkAtomToScope()
        {
            _source?.Get();
        }

        public WidgetViewReferenceType Type => _source?.Value.Type ?? _type;
        public string Path => _source?.Value.Path ?? _path;
        public AssetReferenceGameObject Reference => _source?.Value.Reference ?? _reference;
        public GameObject Prefab => _source?.Value.Prefab is var p && p != null ? p : _prefab;

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
             return $"[{nameof(WidgetViewReference)}: {_type} {_path} {_source}]";
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