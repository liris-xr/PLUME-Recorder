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
            var formattedGuid = new FixedString64Bytes();

            for (var i = 0; i < guid.Length; ++i)
            {
                var rune = guid.Peek(i);

                if (rune == '-')
                    continue;

                if (!IsHexChar(ref rune))
                {
                    throw new FormatException(
                        $"Invalid GUID format {guid}. Expected 32 hex characters but found non-hex char at position {i}.");
                }

                formattedGuid.Append(rune);
            }

            if (formattedGuid.Length != 32)
            {
                throw new FormatException($"Invalid GUID format {guid}. Expected 32 hex characters.");
            }

            _guid = formattedGuid;
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
    }
}