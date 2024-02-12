using ProtoBurst;
using ProtoBurst.Message;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public struct Frame : IProtoBurstMessage
    {
        private static readonly FixedString128Bytes FrameSampleTypeUrl = "fr.liris.plume/fr.liris.plume.Frame";

        public FixedString128Bytes TypeUrl => FrameSampleTypeUrl;

        private readonly int _frameNumber;
        private NativeArray<Any> _data;

        public Frame(int frameNumber, NativeArray<Any> data)
        {
            _frameNumber = frameNumber;
            _data = data;
        }

        [BurstCompile]
        public void WriteTo(ref NativeList<byte> data)
        {
            WritingPrimitives.WriteTag(Sample.Unity.Frame.FrameNumberFieldNumber, WireFormat.WireType.VarInt, ref data);
            WritingPrimitives.WriteInt32(_frameNumber, ref data);

            foreach (var frameData in _data)
            {
                var fd = frameData;
                WritingPrimitives.WriteTag(Sample.Unity.Frame.DataFieldNumber, WireFormat.WireType.LengthDelimited, ref data);
                WritingPrimitives.WriteMessage(ref fd, ref data);
            }
        }
    }
}