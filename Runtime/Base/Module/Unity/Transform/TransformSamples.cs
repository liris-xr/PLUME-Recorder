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
        public static readonly FixedString128Bytes SampleTypeUrl = "fr.liris.plume/" + TransformUpdateLocalPosition.Descriptor.FullName;

        public FixedString128Bytes TypeUrl => SampleTypeUrl;
        
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