using System.Runtime.CompilerServices;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public readonly struct GuidSample : IProtoBurstMessage
    {
        private static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.Guid";

        private readonly uint _x;
        private readonly uint _y;
        private readonly uint _z;
        private readonly uint _w;
        
        private static readonly uint XFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.VarInt);
        private static readonly uint YFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.VarInt);
        private static readonly uint ZFieldTag = WireFormat.MakeTag(3, WireFormat.WireType.VarInt);
        private static readonly uint WFieldTag = WireFormat.MakeTag(4, WireFormat.WireType.VarInt);
        
        public GuidSample(uint x, uint y, uint z, uint w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }

        public int ComputeSize()
        {
            return BufferExtensions.TagSize * 4 +
                   BufferExtensions.ComputeVarIntSize(_x) +
                   BufferExtensions.ComputeVarIntSize(_y) +
                   BufferExtensions.ComputeVarIntSize(_z) +
                   BufferExtensions.ComputeVarIntSize(_w);
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            bufferWriter.WriteTag(XFieldTag);
            bufferWriter.WriteUInt32(_x);
            bufferWriter.WriteTag(YFieldTag);
            bufferWriter.WriteUInt32(_y);
            bufferWriter.WriteTag(ZFieldTag);
            bufferWriter.WriteUInt32(_z);
            bufferWriter.WriteTag(WFieldTag);
            bufferWriter.WriteUInt32(_w);
        }

        [BurstCompile]
        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }
    }
}