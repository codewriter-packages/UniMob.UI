using UniMob.UI.Widgets;
using UnityEngine;

namespace UniMob.UI
{
    public abstract class UniMobUIApp : LifetimeMonoBehaviour
    {
        [SerializeField] private ViewPanel root = default;

        public StateProvider StateProvider { get; } = new StateProvider();

        protected override void Start()
        {
            Initialize();

            UniMobUI.RunApp(Lifetime, StateProvider, root, Build, name);
        }

        protected virtual void Initialize()
        {
        }

        protected abstract Widget Build(BuildContext context);
    }
}