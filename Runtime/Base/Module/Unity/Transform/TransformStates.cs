using PLUME.Core.Object;
using UnityEngine;

namespace PLUME.Base.Module.Unity.Transform
{
    public struct TransformState
    {
        public static TransformState Null => new() { Identifier = ObjectIdentifier.Null };

        public ObjectIdentifier Identifier;

        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;

        // TODO: add dirty flags

        public bool IsNull => Identifier.Equals(ObjectIdentifier.Null);
    }
}