using System;
using System.Text;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;

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

        public Guid(ulong a, ulong b) {
            _a = a;
            _b = b;
        }
        
        public static Guid FromString(string guid)
        {
            var formattedStr = System.Guid.Parse(guid).ToString("N");
            
            ulong a = 0;
            ulong b = 0;
            
            for (var i = 0; i < formattedStr.Length; i++)
            {
                var c = formattedStr[i];

                if (i < 16)
                    a |= (ulong)CharToHex(c) << (i * 4);
                else
                    b |= (ulong)CharToHex(c) << (i % 16 * 4);
            }
            
            return new Guid(a, b);
        }

        [BurstCompile]
        public void WriteTo(ref BufferWriter bufferWriter)
        {
            for(var i = 0; i < 32; i++)
            {
                if (i < 16)
                {
                    var shift = i * 4;
                    var val = (byte) ((_a >> shift) & 0xF);
                    bufferWriter.WriteByte((byte) HexToChar(val));
                }
                else
                {
                    var shift = (i - 16) * 4;
                    var val = (byte) ((_b >> shift) & 0xF);
                    bufferWriter.WriteByte((byte) HexToChar(val));
                }
            }
        }

        [BurstDiscard]
        public override string ToString()
        {
            var sb = new StringBuilder();
            
            for(var i = 0; i < 32; i++)
            {
                if (i < 16)
                {
                    var shift = i * 4;
                    var val = (byte) ((_a >> shift) & 0xF);
                    sb.Append(HexToChar(val));
                }
                else
                {
                    var shift = (i - 16) * 4;
                    var val = (byte) ((_b >> shift) & 0xF);
                    sb.Append(HexToChar(val));
                }
            }
            
            return sb.ToString();
        }

        public bool Equals(Guid other)
        {
            return _a == other._a && _b == other._b;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_a, _b);
        }
        
        private static char HexToChar(byte b)
        {
            if (b < 10)
            {
                return (char)(b + '0');
            }

            return (char)(b - 10 + 'A');
        }

        private static byte CharToHex(char c)
        {
            return c switch
            {
                >= 'A' and <= 'F' => (byte)(c - 'A' + 10),
                >= 'a' and <= 'f' => (byte)(c - 'a' + 10),
                _ => (byte)(c - '0')
            };
        }
    }
}