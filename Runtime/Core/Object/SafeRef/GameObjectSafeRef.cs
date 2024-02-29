using UnityEngine;

namespace PLUME.Core.Object.SafeRef
{
    public class GameObjectSafeRef : IObjectSafeRef<GameObject, GameObjectIdentifier>
    {
        public static readonly GameObjectSafeRef Null = new();

        public readonly GameObjectIdentifier Identifier;

        public readonly GameObject GameObject;

        public readonly ComponentSafeRef<Transform> TransformSafeRef;

        private GameObjectSafeRef()
        {
            Identifier = GameObjectIdentifier.Null;
            TransformSafeRef = ComponentSafeRef<Transform>.Null;
            GameObject = null;
        }

        internal GameObjectSafeRef(GameObject go, Guid goGuid, Guid transformGuid)
        {
            var goIdentifier = new Identifier(go.GetInstanceID(), goGuid);
            var tIdentifier = new Identifier(go.transform.GetInstanceID(), transformGuid);
            Identifier = new GameObjectIdentifier(goIdentifier, tIdentifier);
            GameObject = go;
            TransformSafeRef = new ComponentSafeRef<Transform>(go.transform, transformGuid, this);
        }

        protected bool Equals(GameObjectSafeRef other)
        {
            return Identifier.Equals(other.Identifier);
        }

        public bool Equals(IObjectSafeRef other)
        {
            return other is GameObjectSafeRef gameObjectSafeRef && Equals(gameObjectSafeRef);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((GameObjectSafeRef)obj);
        }

        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }

        public override string ToString()
        {
            return (GameObject == null ? "null" : GameObject.name) + $" ({typeof(GameObject)}): {Identifier}";
        }

        public GameObject GetObject()
        {
            return GameObject;
        }

        public GameObjectIdentifier GetIdentifier()
        {
            return Identifier;
        }
    }
}