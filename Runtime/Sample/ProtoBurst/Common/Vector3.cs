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
    public struct Vector3 : IProtoBurstMessage
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.common.Vector3";

        private static readonly uint XFieldTag =
            WireFormat.MakeTag(Sample.Common.Vector3.XFieldNumber, WireFormat.WireType.Fixed32);

        private static readonly uint YFieldTag =
            WireFormat.MakeTag(Sample.Common.Vector3.YFieldNumber, WireFormat.WireType.Fixed32);

        private static readonly uint ZFieldTag =
            WireFormat.MakeTag(Sample.Common.Vector3.ZFieldNumber, WireFormat.WireType.Fixed32);

        private float X;
        private float Y;
        private float Z;

        public Vector3(float3 vec)
        {
            X = vec.x;
            Y = vec.y;
            Z = vec.z;
        }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
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

            return size;
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }
    }
}