using UnityEngine;

namespace PLUME.Core.Object.SafeRef
{
    public class ComponentSafeRef<TC> : IObjectSafeRef where TC : Component
    {
        public static ComponentSafeRef<TC> Null { get; } =
            new(null, ComponentIdentifier.Null, ObjectSafeRef<GameObject>.Null);

        public TC Component { get; }
        public ComponentIdentifier Identifier { get; }
        public ObjectSafeRef<GameObject> ParentSafeRef { get; }

        internal ComponentSafeRef(TC component, ComponentIdentifier identifier,
            ObjectSafeRef<GameObject> gameObjectSafeRef)
        {
            Component = component;
            Identifier = identifier;
            ParentSafeRef = gameObjectSafeRef;
        }

        public ComponentSafeRef(TC component, Guid guid, ObjectSafeRef<GameObject> gameObjectSafeRef)
        {
            var objectIdentifier = new ObjectIdentifier(component.GetInstanceID(), guid);
            ParentSafeRef = gameObjectSafeRef;
            Identifier = new ComponentIdentifier(objectIdentifier, gameObjectSafeRef.Identifier);
            Component = component;
        }
    }
}