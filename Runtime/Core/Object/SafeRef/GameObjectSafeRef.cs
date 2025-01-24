using System;
using PLUME.Sample.ProtoBurst.Unity;
using UnityEngine;

namespace PLUME.Core.Object.SafeRef
{
    // TODO: make this alloc free
    public class GameObjectSafeRef : IObjectSafeRef<GameObjectIdentifier>, IEquatable<GameObjectSafeRef>
    {
        public static readonly GameObjectSafeRef Null = new();

        public readonly GameObject GameObject;
        public readonly GameObjectIdentifier Identifier;
        public SceneSafeRef SceneSafeRef { get; internal set; }

        public GameObject Object => GameObject;
        UnityEngine.Object IObjectSafeRef.Object => GameObject;
        IObjectIdentifier IObjectSafeRef.Identifier => Identifier;
        GameObjectIdentifier IObjectSafeRef<GameObjectIdentifier>.Identifier => Identifier;

        public bool IsNull => Identifier.Equals(GameObjectIdentifier.Null);

        internal GameObjectSafeRef()
        {
            Identifier = GameObjectIdentifier.Null;
            GameObject = null;
        }

        internal GameObjectSafeRef(GameObject go, Guid guid, Guid transformGuid, SceneSafeRef sceneSafeRef)
        {
            var runtimeId = go.GetInstanceID();
            var transformRuntimeId = go.transform.GetInstanceID();
            Identifier = new GameObjectIdentifier(runtimeId, guid, transformRuntimeId, transformGuid);
            GameObject = go;
            SceneSafeRef = sceneSafeRef;
        }

        public bool Equals(GameObjectSafeRef other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Identifier.Equals(other.Identifier);
        }

        public bool Equals(IObjectSafeRef<GameObjectIdentifier> other)
        {
            return other is GameObjectSafeRef gameObjectSafeRef && Equals(gameObjectSafeRef);
        }

        public bool Equals(IObjectSafeRef other)
        {
            return other is GameObjectSafeRef gameObjectSafeRef && Equals(gameObjectSafeRef);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GameObjectSafeRef)obj);
        }

        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }

        public override string ToString()
        {
            return (GameObject == null ? "null" : GameObject.name) + $" ({typeof(GameObject)}): {Identifier}";
        }
    }
}