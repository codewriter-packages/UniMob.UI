using UniMob.UI.Widgets;
using UnityEngine;

namespace UniMob.UI
{
    public abstract class UniMobUIApp : LifetimeMonoBehaviour
    {
        [SerializeField] private ViewPanel root = default;

        protected override void Start()
        {
            Initialize();

            UniMobUI.RunApp(Lifetime, root, Build, name);
        }

        protected virtual void Initialize()
        {
        }

        protected abstract Widget Build(BuildContext context);
    }
}