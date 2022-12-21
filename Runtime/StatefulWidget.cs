using System;
using JetBrains.Annotations;

namespace UniMob.UI
{
    public abstract class StatefulWidget : Widget
    {
        private Type _type;

        public Type Type => _type ?? (_type = GetType());

        [CanBeNull] public Key Key { get; set; }

        [NotNull]
        public virtual State CreateState(StateProvider provider)
        {
            return provider.Of(this);
        }

        [CanBeNull]
        public virtual State CreateState()
        {
            return null;
        }
    }
}