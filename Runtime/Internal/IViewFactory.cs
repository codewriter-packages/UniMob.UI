using System;
using JetBrains.Annotations;
using UnityEngine;

namespace UniMob.UI.Internal
{
    internal interface IViewFactory
    {
        string Name { get; }

        GameObject Create();
    }

    internal abstract class RegisterViewFactoryAttribute : Attribute
    {
        public abstract IViewFactory CreateFactory();
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal class RegisterCustomViewFactoryAttribute : RegisterViewFactoryAttribute
    {
        public Type FactoryType { get; }

        public RegisterCustomViewFactoryAttribute([NotNull] Type factoryType)
        {
            FactoryType = factoryType ?? throw new ArgumentNullException(nameof(factoryType));
        }

        public override IViewFactory CreateFactory()
        {
            return Activator.CreateInstance(FactoryType) as IViewFactory;
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal class RegisterComponentViewFactoryAttribute : RegisterViewFactoryAttribute
    {
        public string Name { get; }
        public Type[] Components { get; }

        public RegisterComponentViewFactoryAttribute(string name, params Type[] components)
        {
            Name = name;
            Components = components;
        }

        public override IViewFactory CreateFactory()
        {
            return new ComponentViewFactory(Name, Components);
        }
    }

    internal sealed class ComponentViewFactory : IViewFactory
    {
        public ComponentViewFactory(string name, Type[] components)
        {
            Name = name;
            NiceName = name.StartsWith("$$_") ? name.Substring(3) : name;
            Components = components;
        }

        public string Name { get; }
        public string NiceName { get; }
        public Type[] Components { get; }

        public GameObject Create()
        {
            var go = new GameObject(NiceName);
            go.SetActive(false);
            
            foreach (var component in Components)
            {
                var added = go.AddComponent(component);

                if (component == typeof(RectTransform))
                {
                    ((RectTransform) added).sizeDelta = Vector2.zero;
                }
            }
            
            return go;
        }
    }
}