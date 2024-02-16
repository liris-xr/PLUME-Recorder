using System.Runtime.CompilerServices;
using PLUME.Sample.Common;
using ProtoBurst;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public struct Vector3Sample : IProtoBurstMessage
    {
        public static readonly FixedString128Bytes SampleTypeUrl = "fr.liris.plume/" + Vector3.Descriptor.FullName;
        public FixedString128Bytes TypeUrl => SampleTypeUrl;

        public float X;
        public float Y;
        public float Z;

        public Vector3Sample(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteToNoResize(ref NativeList<byte> data)
        {
            if (X != 0)
            {
                WritingPrimitives.WriteTagNoResize(Vector3.XFieldNumber, WireFormat.WireType.Fixed32, ref data);
                WritingPrimitives.WriteFloatNoResize(X, ref data);
            }

            if (Y != 0)
            {
                WritingPrimitives.WriteTagNoResize(Vector3.YFieldNumber, WireFormat.WireType.Fixed32, ref data);
                WritingPrimitives.WriteFloatNoResize(Y, ref data);
            }

            if (Z != 0)
            {
                WritingPrimitives.WriteTagNoResize(Vector3.ZFieldNumber, WireFormat.WireType.Fixed32, ref data);
                WritingPrimitives.WriteFloatNoResize(Z, ref data);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ComputeMaxSize()
        {
            return (WritingPrimitives.TagSize + WritingPrimitives.Fixed32Size) * 3;
        }
    }
}