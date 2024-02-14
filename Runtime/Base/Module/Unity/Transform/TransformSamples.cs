using System.Runtime.CompilerServices;
using PLUME.Core.Recorder.ProtoBurst;
using PLUME.Sample.Unity;
using ProtoBurst;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Base.Module.Unity.Transform
{
    [BurstCompile]
    public struct TransformUpdateLocalPositionSample : IProtoBurstMessage
    {
        public FixedString128Bytes TypeUrl => "fr.liris.plume/plume.sample.unity.TransformUpdateLocation";
        
        // TODO: add identifier

        public Vector3Sample LocalPosition;

        public TransformUpdateLocalPositionSample(Vector3Sample localPosition)
        {
            LocalPosition = localPosition;
        }

        // TODO: return length written
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteToNoResize(ref NativeList<byte> data)
        {
            WritingPrimitives.WriteTagNoResize(TransformUpdateLocalPosition.LocalPositionFieldNumber,
                WireFormat.WireType.LengthDelimited, ref data);
            WritingPrimitives.WriteLengthPrefixedMessageNoResize(ref LocalPosition, ref data);
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ComputeMaxSize()
        {
            return WritingPrimitives.TagSize + WritingPrimitives.LengthPrefixMaxSize + LocalPosition.ComputeMaxSize();
        }
    }
}