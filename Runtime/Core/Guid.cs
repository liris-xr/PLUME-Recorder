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
    public struct Guid : IEquatable<Guid>
    {
        // 32 hex characters
        public const int Size = 32 * sizeof(byte);

        public static Guid Null { get; } = new("00000000000000000000000000000000");

        private FixedString64Bytes _guid;

        public Guid(FixedString64Bytes guid)
        {
            if (guid.Length != 32)
            {
                throw new FormatException($"Invalid GUID format {guid}. Expected 32 hex characters.");
            }

            _guid = guid;
        }

        [BurstCompile]
        private static bool IsHexChar(ref Unicode.Rune rune)
        {
            return rune.value is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
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

        public bool Equals(Guid other)
        {
            return _guid.Equals(other._guid);
        }

        [BurstDiscard]
        public override bool Equals(object obj)
        {
            return obj is Guid other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }
        
        public static bool operator ==(Guid left, Guid right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(Guid left, Guid right)
        {
            return !left.Equals(right);
        }
    }
}