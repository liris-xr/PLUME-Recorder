using System;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Profiling;

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
        
        public static Guid Null { get; } = new(0, 0);

        private readonly ulong _a;
        private readonly ulong _b;
        private FixedString64Bytes _utf8String;

        public Guid(ulong a, ulong b) {
            _a = a;
            _b = b;
            _utf8String = new FixedString64Bytes(_a.ToString("X16") + _b.ToString("X16"));
        }
        
        public static Guid FromString(string str)
        {
            var systemGuid = System.Guid.Parse(str);
            return FromSystemGuid(systemGuid);
        }
        
        public static Guid FromSystemGuid(System.Guid systemGuid)
        {
            var bytes = systemGuid.ToByteArray();
            var a = BitConverter.ToUInt64(bytes, 0);
            var b = BitConverter.ToUInt64(bytes, 8);
            return new Guid(a, b);
        }

        [BurstCompile]
        private static bool IsHexChar(ref Unicode.Rune rune)
        {
            return rune.value is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
        }

        [BurstCompile]
        public void WriteTo(ref BufferWriter bufferWriter)
        {
            bufferWriter.WriteFixedString(ref _utf8String);
        }

        [BurstDiscard]
        public override string ToString()
        {
            return _utf8String.ToString();
        }

        public bool Equals(Guid other)
        {
            return _a == other._a && _b == other._b;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_a, _b);
        }
    }
}