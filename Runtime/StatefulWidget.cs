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
        public abstract State CreateState();
    }
}