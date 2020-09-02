namespace UniMob.UI
{
    public class BuildContext
    {
        public IState State { get; }

        public BuildContext Parent { get; protected set; }

        public BuildContext(IState state, BuildContext parent)
        {
            State = state;
            Parent = parent;
        }

        public virtual TState AncestorStateOfType<TState>()
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

        public virtual TState RootAncestorStateOfType<TState>()
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
        public MutableBuildContext(IState state, BuildContext parent)
            : base(state, parent)
        {
        }

        public void SetParent(BuildContext parent)
        {
            Parent = parent;
        }
    }
}