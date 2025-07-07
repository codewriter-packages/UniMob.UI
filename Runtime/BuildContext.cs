using System.Xml.Linq;

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

        internal T? FindAncestorStateImplementing<T>() where T : class
        {
            var ancestor = this;
            
            while (ancestor != null)
            {
                if (ancestor.State is T implementingState)
                {
                    return implementingState;
                }
                ancestor = ancestor.Parent;
            }

            return null;
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