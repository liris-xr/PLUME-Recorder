using ProtoBurst;
using ProtoBurst.Message;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public struct FrameSample : IProtoBurstMessage
    {
        public FixedString128Bytes TypeUrl => "fr.liris.plume/plume.sample.unity.Frame";

        public readonly int FrameNumber;
        public NativeArray<Any> Data;

        public FrameSample(int frameNumber, NativeArray<Any> data)
        {
            FrameNumber = frameNumber;
            Data = data;
        }

        [BurstCompile]
        public void WriteTo(ref NativeList<byte> data)
        {
            WritingPrimitives.WriteTag(Sample.Unity.Frame.FrameNumberFieldNumber, WireFormat.WireType.VarInt, ref data);
            WritingPrimitives.WriteInt32(FrameNumber, ref data);

            foreach (var frameData in Data)
            {
                var fd = frameData;
                WritingPrimitives.WriteTag(Sample.Unity.Frame.DataFieldNumber, WireFormat.WireType.LengthDelimited,
                    ref data);
                WritingPrimitives.WriteMessage(ref fd, ref data);
            }
        }
    }
}