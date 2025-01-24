using PLUME.Sample.ProtoBurst.Unity;
using UnityEngine;

namespace PLUME.Core.Object.SafeRef
{
    public interface IComponentSafeRef : IObjectSafeRef<ComponentIdentifier>
    {
        public Component Component { get; }

        public GameObjectSafeRef GameObjectSafeRef { get; }
    }

    public interface IComponentSafeRef<out TC> : IComponentSafeRef where TC : Component
    {
        public new TC Component { get; }
    }

    // TODO: make this alloc free
    public class ComponentSafeRef<TC> : IComponentSafeRef<TC> where TC : Component
    {
        public static readonly ComponentSafeRef<TC> Null = new();

        public TC Component { get; }
        public ComponentIdentifier Identifier { get; }
        public GameObjectSafeRef GameObjectSafeRef { get; }

        Component IComponentSafeRef.Component => Component;
        UnityEngine.Object IObjectSafeRef.Object => Component;
        IObjectIdentifier IObjectSafeRef.Identifier => Identifier;

        public bool IsNull => Identifier.Equals(ComponentIdentifier.Null);

        internal ComponentSafeRef()
        {
            Component = null;
            Identifier = ComponentIdentifier.Null;
            GameObjectSafeRef = new GameObjectSafeRef();
        }

        internal ComponentSafeRef(TC component, Guid guid, GameObjectSafeRef gameObjectSafeRef)
        {
            Component = component;
            Identifier = new ComponentIdentifier(component.GetInstanceID(), guid, gameObjectSafeRef.Identifier);
            GameObjectSafeRef = gameObjectSafeRef;
        }

        public bool Equals(ComponentSafeRef<TC> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Identifier.Equals(other.Identifier);
        }

        public bool Equals(IObjectSafeRef<ComponentIdentifier> other)
        {
            return other is ComponentSafeRef<TC> componentSafeRef && Equals(componentSafeRef);
        }

        public bool Equals(IObjectSafeRef other)
        {
            return other is ComponentSafeRef<TC> componentSafeRef && Equals(componentSafeRef);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ComponentSafeRef<TC>)obj);
        }

        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }
    }
}