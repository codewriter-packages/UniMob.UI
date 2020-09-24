using System;
using UniMob.UI.Widgets;
using UnityEngine;

namespace UniMob.UI
{
    public abstract class UniMobUIApp : MonoBehaviour
    {
        [SerializeField] private ViewPanel root = default;

        private IDisposable _render;

        protected virtual void OnEnable()
        {
            Initialize();

            _render = UniMobUI.RunApp(root, Build, name);
        }

        protected virtual void OnDisable()
        {
            _render.Dispose();
        }

        protected virtual void Initialize()
        {
        }

        protected abstract Widget Build(BuildContext context);
    }
}