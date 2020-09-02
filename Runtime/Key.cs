using System;
using JetBrains.Annotations;

namespace UniMob.UI
{
    public abstract class Key : IEquatable<Key>
    {
        public abstract bool Equals(Key other);
        public sealed override bool Equals(object obj) => Equals(obj as Key);
        public override int GetHashCode() => throw new InvalidOperationException();

        public static Key Of([NotNull] object value) => new ObjectKey(value);

        public static bool operator ==(Key a, Key b) => a?.Equals(b) ?? ReferenceEquals(b, null);
        public static bool operator !=(Key a, Key b) => !a?.Equals(b) ?? !ReferenceEquals(b, null);
    }

    internal sealed class ObjectKey : Key, IEquatable<ObjectKey>
    {
        public object Value { get; }

        internal ObjectKey([NotNull] object value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override bool Equals(Key other) => Equals(other as ObjectKey);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => $"[Key: {Value}]";

        public bool Equals(ObjectKey other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(other.Value, Value))
                return true;

            return Value.Equals(other.Value);
        }
    }

    public sealed class GlobalKey<T> : GlobalKey, IEquatable<GlobalKey<T>>
        where T : class, IState
    {
        public static readonly GlobalKey<T> Instance = new GlobalKey<T>();

        private GlobalKey()
        {
        }

        public override bool Equals(Key other) => Equals(other as GlobalKey<T>);
        public override int GetHashCode() => typeof(T).GetHashCode();
        public override string ToString() => $"[GlobalKey: {typeof(T)}]";

        public bool Equals(GlobalKey<T> other) => ReferenceEquals(other, this);

        public T CurrentState => UntypedCurrentState is T state ? state : default;
    }

    public abstract class GlobalKey : Key
    {
        public static GlobalKey<T> Of<T>() where T : class, IState => GlobalKey<T>.Instance;

        internal State UntypedCurrentState { get; set; }

        public Widget CurrentRawWidget => UntypedCurrentState?.RawWidget;

        public BuildContext CurrentContext => UntypedCurrentState?.Context;
    }
}