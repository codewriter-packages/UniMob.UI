using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniMob.UI.Internal.ViewLoaders
{
    public class InternalViewLoader : IViewLoader
    {
        private readonly Dictionary<string, Func<GameObject>> _builders = new Dictionary<string, Func<GameObject>>();
        private readonly Dictionary<string, IView> _cache = new Dictionary<string, IView>();

        public InternalViewLoader()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var attributes = assembly.GetCustomAttributes(typeof(RegisterViewFactoryAttribute), false);
                foreach (RegisterViewFactoryAttribute attribute in attributes)
                {
                    var factory = attribute.CreateFactory();
                    if (factory == null)
                    {
                        continue;
                    }

                    var name = factory.Name;
                    if (_builders.ContainsKey(name))
                    {
                        Debug.LogError($"Multiple view factory for {name}");
                        continue;
                    }

                    _builders.Add(name, factory.Create);
                }
            }
        }

        public (IView, WidgetViewReference) LoadViewPrefab(IViewState state)
        {
            var viewReference = state.View;

            if (viewReference.Type != WidgetViewReferenceType.Resource ||
                !viewReference.Path.StartsWith("$$_"))
            {
                return (null, default);
            }

            var name = viewReference.Path;

            if (_cache.TryGetValue(name, out var view))
            {
                return (view, viewReference);
            }

            if (!_builders.TryGetValue(name, out var builder))
            {
                throw new InvalidOperationException("No builder");
            }

            var template = builder();
            Object.DontDestroyOnLoad(template);
            template.hideFlags = HideFlags.HideInHierarchy;

            view = template.GetComponent<IView>();

            _cache.Add(name, view);

            return (view, viewReference);
        }
    }
}