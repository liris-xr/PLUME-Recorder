using System.Runtime.InteropServices;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace PLUME.Sample.ProtoBurst.Common
{
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    public struct Quaternion : IProtoBurstMessage
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.common.Quaternion";

        private static readonly uint XFieldTag =
            WireFormat.MakeTag(Sample.Common.Quaternion.XFieldNumber, WireFormat.WireType.Fixed32);

        private static readonly uint YFieldTag =
            WireFormat.MakeTag(Sample.Common.Quaternion.YFieldNumber, WireFormat.WireType.Fixed32);

        private static readonly uint ZFieldTag =
            WireFormat.MakeTag(Sample.Common.Quaternion.ZFieldNumber, WireFormat.WireType.Fixed32);

        private static readonly uint WFieldTag =
            WireFormat.MakeTag(Sample.Common.Quaternion.WFieldNumber, WireFormat.WireType.Fixed32);

        private float X;
        private float Y;
        private float Z;
        private float W;

        public Quaternion(quaternion vec)
        {
            X = vec.value.x;
            Y = vec.value.y;
            Z = vec.value.z;
            W = vec.value.w;
        }

        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            if (X != 0)
            {
                bufferWriter.WriteTag(XFieldTag);
                bufferWriter.WriteFloat(X);
            }

            if (Y != 0)
            {
                bufferWriter.WriteTag(YFieldTag);
                bufferWriter.WriteFloat(Y);
            }

            if (Z != 0)
            {
                bufferWriter.WriteTag(ZFieldTag);
                bufferWriter.WriteFloat(Z);
            }

            if (W != 0)
            {
                bufferWriter.WriteTag(WFieldTag);
                bufferWriter.WriteFloat(W);
            }
        }

        public int ComputeSize()
        {
            var size = 0;

            if (X != 0)
            {
                size += BufferWriterExtensions.ComputeTagSize(XFieldTag) + BufferWriterExtensions.Fixed32Size;
            }

            if (Y != 0)
            {
                size += BufferWriterExtensions.ComputeTagSize(YFieldTag) + BufferWriterExtensions.Fixed32Size;
            }

            if (Z != 0)
            {
                size += BufferWriterExtensions.ComputeTagSize(ZFieldTag) + BufferWriterExtensions.Fixed32Size;
            }

            if (W != 0)
            {
                size += BufferWriterExtensions.ComputeTagSize(WFieldTag) + BufferWriterExtensions.Fixed32Size;
            }

            return size;
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }
    }
}