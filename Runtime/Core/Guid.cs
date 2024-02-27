using System;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Core
{
    /// <summary>
    /// A 128-bit globally unique identifier.
    /// </summary>
    [BurstCompile]
    public struct Guid
    {
        // 32 hex characters + 4 hyphens
        public const int Size = 36 * sizeof(byte);
        
        public static Guid Null { get; } = new("00000000-0000-0000-0000-000000000000");

        private FixedString64Bytes _guid;

        public Guid(FixedString64Bytes guid)
        {
            if(guid.Length != 36)
                throw new FormatException("Invalid GUID format");
            
            _guid = guid;
        }
        
        [BurstCompile]
        public void WriteTo(ref BufferWriter bufferWriter)
        {
            bufferWriter.WriteFixedString(ref _guid);
        }
        
        [BurstDiscard]
        public override string ToString()
        {
            return _guid.ToString();
        }
    }
}