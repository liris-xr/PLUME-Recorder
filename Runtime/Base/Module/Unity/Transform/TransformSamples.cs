using System.Runtime.CompilerServices;
using PLUME.Core.Recorder.ProtoBurst;
using PLUME.Sample.Unity;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Base.Module.Unity.Transform
{
    [BurstCompile]
    public struct TransformUpdateLocalPositionSample : IProtoBurstMessage
    {
        public static readonly SampleTypeUrl LocalPositionTypeUrl =
            SampleTypeUrlRegistry.GetOrCreate("fr.liris.plume", TransformUpdateLocalPosition.Descriptor);

        public SampleTypeUrl TypeUrl => LocalPositionTypeUrl;

        // TODO: add identifier

        public Vector3Sample LocalPosition;

        public TransformUpdateLocalPositionSample(Vector3Sample localPosition)
        {
            LocalPosition = localPosition;
        }

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
        public void WriteTo(ref NativeList<byte> data)
        {
            WritingPrimitives.WriteTag(TransformUpdateLocalPosition.LocalPositionFieldNumber,
                WireFormat.WireType.LengthDelimited, ref data);
            WritingPrimitives.WriteLengthPrefixedMessage(ref LocalPosition, ref data);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ComputeMaxSize()
        {
            return WritingPrimitives.TagSize + WritingPrimitives.LengthPrefixMaxSize + LocalPosition.ComputeMaxSize();
        }
    }
}