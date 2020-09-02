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

        private WidgetViewReference(WidgetViewReferenceType type, string path, AssetReferenceGameObject reference)
        {
            _type = type;
            _path = path;
            _reference = reference;
            _source = null;
        }

        internal WidgetViewReference(Atom<WidgetViewReference> source)
        {
            _type = WidgetViewReferenceType.Resource;
            _path = null;
            _reference = null;
            _source = source;
        }

        internal void LinkAtomToScope()
        {
            _source?.Get();
        }

        public WidgetViewReferenceType Type => _source?.Value.Type ?? _type;
        public string Path => _source?.Value.Path ?? _path;
        public AssetReferenceGameObject Reference => _source?.Value.Reference ?? _reference;

        public bool Equals(WidgetViewReference other)
        {
            return Type == other.Type && Path == other.Path;
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
            return new WidgetViewReference(WidgetViewReferenceType.Addressable, path, null);
        }
        
        public static WidgetViewReference Addressable(AssetReferenceGameObject reference)
        {
            return new WidgetViewReference(WidgetViewReferenceType.Addressable, null, reference);
        }

        public static WidgetViewReference Resource(string path)
        {
            return new WidgetViewReference(WidgetViewReferenceType.Resource, path, null);
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
    }
}