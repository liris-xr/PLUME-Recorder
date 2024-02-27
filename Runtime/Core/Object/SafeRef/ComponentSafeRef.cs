using UnityEngine;

namespace PLUME.Core.Object.SafeRef
{
    public class ComponentSafeRef<TC> : IObjectSafeRef<TC, ComponentIdentifier> where TC : Component
    {
        public static ComponentSafeRef<TC> Null { get; } = new(null, ComponentIdentifier.Null, GameObjectSafeRef.Null);

        public TC Component { get; }
        public ComponentIdentifier ComponentIdentifier { get; }
        public GameObjectSafeRef ParentSafeRef { get; }

        internal ComponentSafeRef(TC component, ComponentIdentifier componentIdentifier, GameObjectSafeRef gameObjectSafeRef)
        {
            Component = component;
            ComponentIdentifier = componentIdentifier;
            ParentSafeRef = gameObjectSafeRef;
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