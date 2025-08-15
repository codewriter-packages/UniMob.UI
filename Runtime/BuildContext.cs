using UniMob.UI.Layout;

namespace UniMob.UI
{
    public class BuildContext
    {
        public IState State { get; }

        public BuildContext Parent { get; protected set; }

        private readonly LayoutConstraints? _explicitConstraints;

        public LayoutConstraints Constraints => _explicitConstraints ?? Parent?.Constraints ?? default;

        public BuildContext(IState state, BuildContext parent, LayoutConstraints? constraints = null)
        {
            State = state;
            Parent = parent;
            _explicitConstraints = constraints;
        }

        public TState AncestorStateOfType<TState>()
            where TState : IState
        {
            var ancestor = this;

            while (ancestor != null)
            {
                if (ancestor.State is TState state)
                {
                    return state;
                }

                ancestor = ancestor.Parent;
            }

            return default;
        }

        public TState RootAncestorStateOfType<TState>()
            where TState : IState
        {
            var ancestor = this;

            TState root = default;
            while (ancestor != null)
            {
                if (ancestor.State is TState state)
                {
                    root = state;
                }

                ancestor = ancestor.Parent;
            }

            return root;
        }
    }

    public class MutableBuildContext : BuildContext
    {
        public MutableBuildContext(IState state, BuildContext parent, LayoutConstraints? constraints = null)
            : base(state, parent, constraints)
        {
        }

        public void SetParent(BuildContext parent)
        {
            Parent = parent;
        }
    }
}