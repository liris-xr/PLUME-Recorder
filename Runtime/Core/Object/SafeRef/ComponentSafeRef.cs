using UnityEngine;

namespace PLUME.Core.Object.SafeRef
{
    public class ComponentSafeRef<TC> : IObjectSafeRef<TC, ComponentIdentifier> where TC : Component
    {
        public static readonly ComponentSafeRef<TC> Null = new();

        public readonly TC Component;
        public readonly ComponentIdentifier ComponentIdentifier;
        public readonly GameObjectSafeRef ParentSafeRef;

        private ComponentSafeRef()
        {
            Component = null;
            ComponentIdentifier = ComponentIdentifier.Null;
            ParentSafeRef = GameObjectSafeRef.Null;
        }

        internal ComponentSafeRef(TC component, Guid guid, GameObjectSafeRef gameObjectSafeRef)
        {
            var objectIdentifier = new Identifier(component.GetInstanceID(), guid);
            ParentSafeRef = gameObjectSafeRef;
            ComponentIdentifier = new ComponentIdentifier(objectIdentifier, gameObjectSafeRef.Identifier);
            Component = component;
        }
        
        public TC GetObject()
        {
            return Component;
        }

        public ComponentIdentifier GetIdentifier()
        {
            return ComponentIdentifier;
        }
    }
}