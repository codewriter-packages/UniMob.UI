using System;
using JetBrains.Annotations;

namespace UniMob.UI
{
    public abstract class StatefulWidget : Widget
    {
        private Type _type;

        protected StatefulWidget([CanBeNull] Key key = null)
        {
            Key = key;
        }

        public Type Type => _type ?? (_type = GetType());

        [CanBeNull] public Key Key { get; }

        [NotNull]
        public abstract State CreateState();
    }
}