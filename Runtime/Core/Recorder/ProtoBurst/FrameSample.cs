using System.Runtime.CompilerServices;
using ProtoBurst;
using ProtoBurst.Message;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public struct FrameSample : IProtoBurstMessage
    {
        public static FixedString128Bytes FrameSampleTypeUrl => "fr.liris.plume/plume.sample.unity.Frame";
        public FixedString128Bytes TypeUrl => FrameSampleTypeUrl;

        public readonly int FrameNumber;
        public NativeArray<Any> Data;

        public FrameSample(int frameNumber, NativeArray<Any> data)
        {
            FrameNumber = frameNumber;
            Data = data;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteToNoResize(ref NativeList<byte> data)
        {
            WritingPrimitives.WriteTagNoResize(Sample.Unity.Frame.FrameNumberFieldNumber, WireFormat.WireType.VarInt, ref data);
            WritingPrimitives.WriteInt32NoResize(FrameNumber, ref data);

            foreach (var frameData in Data)
            {
                var fd = frameData;
                WritingPrimitives.WriteTagNoResize(Sample.Unity.Frame.DataFieldNumber, WireFormat.WireType.LengthDelimited,
                    ref data);
                WritingPrimitives.WriteLengthPrefixedMessageNoResize(ref fd, ref data);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ComputeMaxSize()
        {
            var size = 0;
            size += sizeof(ushort);
            size += sizeof(uint);
            
            foreach (var frameData in Data)
            {
                size += sizeof(ushort);
                size += sizeof(uint);
                size += frameData.ComputeMaxSize();
            }

            return size;
        }
    }
}