using UnityEngine;

namespace PLUME.Recorder.Module.Unity.Transform
{
    public struct TransformState : IUnityObjectState
    {
        public static TransformState Null => new() { Identifier = ObjectIdentifier.Null };

        public ObjectIdentifier Identifier;

        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;
        
        // TODO: add dirty flags
        
        public bool IsNull => Identifier.Equals(ObjectIdentifier.Null);
    }

    public struct TransformCreatedState : IUnityObjectState
    {
        public int InstanceId;

        public TransformCreatedState(int instanceId)
        {
            InstanceId = instanceId;
        }
    }

    public struct TransformDestroyedState : IUnityObjectState
    {
        public int InstanceId;

        public TransformDestroyedState(int instanceId)
        {
            InstanceId = instanceId;
        }
    }
}