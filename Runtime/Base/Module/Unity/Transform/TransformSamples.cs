using PLUME.Core.Recorder.ProtoBurst;
using PLUME.Sample.Unity;
using ProtoBurst;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Profiling;

namespace PLUME.Base.Module.Unity.Transform
{
    [BurstCompile]
    public struct TransformUpdateLocalPositionSample : IProtoBurstMessage
    {
        public FixedString128Bytes TypeUrl => "fr.liris.plume/plume.sample.unity.TransformUpdateLocation";

        public static int MaxSize => sizeof(ushort) + sizeof(uint) + Vector3Sample.MaxSize;
        
        // TODO: add identifier

        public Vector3Sample LocalPosition;

        public TransformUpdateLocalPositionSample(Vector3Sample localPosition)
        {
            LocalPosition = localPosition;
        }

        // TODO: return length written
        public void WriteTo(ref NativeList<byte> data)
        {
            WritingPrimitives.WriteTag(TransformUpdateLocalPosition.LocalPositionFieldNumber,
                WireFormat.WireType.LengthDelimited, ref data);
            WritingPrimitives.WriteMessage(ref LocalPosition, ref data);
        }
    }
}