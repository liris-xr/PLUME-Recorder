using PLUME.Sample.Common;
using ProtoBurst;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public struct Vector3Sample : IProtoBurstMessage
    {
        public FixedString128Bytes TypeUrl => "fr.liris.plume/plume.sample.common.Vector3";
        
        public static int MaxSize => (sizeof(ushort) + sizeof(float)) * 3;

        public float X;
        public float Y;
        public float Z;

        public Vector3Sample(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void WriteTo(ref NativeList<byte> data)
        {
            if (X != 0)
            {
                WritingPrimitives.WriteTag(Vector3.XFieldNumber, WireFormat.WireType.Fixed32, ref data);
                WritingPrimitives.WriteFloat(X, ref data);
            }
            
            if (Y != 0)
            {
                WritingPrimitives.WriteTag(Vector3.YFieldNumber, WireFormat.WireType.Fixed32, ref data);
                WritingPrimitives.WriteFloat(Y, ref data);
            }
            
            if (Z != 0)
            {
                WritingPrimitives.WriteTag(Vector3.ZFieldNumber, WireFormat.WireType.Fixed32, ref data);
                WritingPrimitives.WriteFloat(Z, ref data);
            }
        }
    }
}