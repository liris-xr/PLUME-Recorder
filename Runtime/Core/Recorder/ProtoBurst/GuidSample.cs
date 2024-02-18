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
        private static readonly SampleTypeUrl GuidTypeUrl =
            SampleTypeUrlRegistry.GetOrCreate("fr.liris.plume/plume.sample.Guid");
        
        public SampleTypeUrl TypeUrl => GuidTypeUrl;
        
        private readonly uint _x;
        private readonly uint _y;
        private readonly uint _z;
        private readonly uint _w;

        public GuidSample(uint x, uint y, uint z, uint w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ComputeMaxSize()
        {
            return WritingPrimitives.TagSize * 4 + WritingPrimitives.VarInt32MaxSize * 4;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteToNoResize(ref NativeList<byte> data)
        {
            WritingPrimitives.WriteTagNoResize(1, WireFormat.WireType.VarInt, ref data);
            WritingPrimitives.WriteUInt32NoResize(_x, ref data);
            WritingPrimitives.WriteTagNoResize(2, WireFormat.WireType.VarInt, ref data);
            WritingPrimitives.WriteUInt32NoResize(_y, ref data);
            WritingPrimitives.WriteTagNoResize(3, WireFormat.WireType.VarInt, ref data);
            WritingPrimitives.WriteUInt32NoResize(_z, ref data);
            WritingPrimitives.WriteTagNoResize(4, WireFormat.WireType.VarInt, ref data);
            WritingPrimitives.WriteUInt32NoResize(_w, ref data);
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteTo(ref NativeList<byte> data)
        {
            WritingPrimitives.WriteTag(1, WireFormat.WireType.VarInt, ref data);
            WritingPrimitives.WriteUInt32(_x, ref data);
            WritingPrimitives.WriteTag(2, WireFormat.WireType.VarInt, ref data);
            WritingPrimitives.WriteUInt32(_y, ref data);
            WritingPrimitives.WriteTag(3, WireFormat.WireType.VarInt, ref data);
            WritingPrimitives.WriteUInt32(_z, ref data);
            WritingPrimitives.WriteTag(4, WireFormat.WireType.VarInt, ref data);
            WritingPrimitives.WriteUInt32(_w, ref data);
        }
    }
}