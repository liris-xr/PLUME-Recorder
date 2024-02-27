using UnityEngine;

namespace PLUME.Core.Object.SafeRef
{
    public class GameObjectSafeRef : IObjectSafeRef<GameObject, GameObjectIdentifier>
    {
        public static GameObjectSafeRef Null { get; } =
            new(GameObjectIdentifier.Null, ComponentSafeRef<Transform>.Null);

        public GameObjectIdentifier Identifier { get; }

        public readonly GameObject GameObject;

        public ComponentSafeRef<Transform> TransformSafeRef { get; internal set; }

        internal GameObjectSafeRef(GameObjectIdentifier identifier, ComponentSafeRef<Transform> transformSafeRef)
        {
            Identifier = identifier;
            TransformSafeRef = transformSafeRef;
            GameObject = null;
        }

        internal GameObjectSafeRef(GameObject go, Guid guid, ComponentSafeRef<Transform> transformSafeRef)
        {
            var identifier = new Identifier(go.GetInstanceID(), guid);
            var transformIdentifier = new Identifier(go.transform.GetInstanceID(), guid);
            Identifier = new GameObjectIdentifier(identifier, transformIdentifier);
            GameObject = go;
            TransformSafeRef = transformSafeRef;
        }

        public bool Equals(GameObjectSafeRef other)
        {
            return Identifier == other.Identifier;
        }

        public override bool Equals(object obj)
        {
            return obj is GameObjectSafeRef other && Equals(other);
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