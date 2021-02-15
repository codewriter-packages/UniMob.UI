using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UniMob.UI.Internal;

namespace UniMob.UI
{
    internal sealed class ClassPool<TType>
    {
        private readonly Stack<TType> _pool = new Stack<TType>();
        private readonly Func<TType> _create;
        private readonly Action<TType> _prepare;
        private readonly Action<TType> _recycle;

        public ClassPool([NotNull] Func<TType> create, [NotNull] Action<TType> prepare, [NotNull] Action<TType> recycle)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
            _prepare = prepare ?? throw new ArgumentNullException(nameof(prepare));
            _recycle = recycle ?? throw new ArgumentNullException(nameof(recycle));
        }

        [NotNull] public TType Get()
        {
            var item = _pool.Count > 0 ? _pool.Pop() : _create();
            _prepare(item);
            return item;
        }

        public void Recycle([NotNull] TType item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            _recycle(item);
            _pool.Push(item);
        }
    }

    internal static class Pools
    {
        public static readonly ClassPool<Dictionary<Key, State>> KeyToState = new ClassPool<Dictionary<Key, State>>(
            () => new Dictionary<Key, State>(),
            o => { },
            o => o.Clear());
    }
}