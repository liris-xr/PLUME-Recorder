using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace PLUME.Sample.ProtoBurst.Unity
{
    /// <summary>
    /// Blittable descriptor of a line renderer positions update. The positions live in a shared per-frame
    /// <see cref="NativeList{T}"/> owned by the frame data; this struct only references them by
    /// <see cref="PositionsOffset"/> / <see cref="PositionsCount"/>, so any number of positions is serialized with no
    /// managed allocation and no per-position object.
    /// </summary>
    [BurstCompile]
    public struct LineRendererPositionsUpdate
    {
        public static readonly FixedString128Bytes TypeUrl =
            "fr.liris.plume/plume.sample.unity.LineRendererPositionsUpdate";

        public static readonly uint ComponentFieldTag =
            WireFormat.MakeTag(Sample.Unity.LineRendererPositionsUpdate.ComponentFieldNumber,
                WireFormat.WireType.LengthDelimited);

        public static readonly uint PositionsFieldTag =
            WireFormat.MakeTag(Sample.Unity.LineRendererPositionsUpdate.PositionsFieldNumber,
                WireFormat.WireType.LengthDelimited);

        public ComponentIdentifier Component;
        public int PositionsOffset;
        public int PositionsCount;

        /// <summary>Serialized size of the message body (component + packed xyz floats), excluding the Any envelope.</summary>
        public int ComputeMessageSize()
        {
            var component = Component;
            var size = BufferWriterExtensions.ComputeTagSize(ComponentFieldTag) +
                       BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref component);

            if (PositionsCount > 0)
            {
                var positionsBytes = PositionsCount * 3 * BufferWriterExtensions.Fixed32Size;
                size += BufferWriterExtensions.ComputeTagSize(PositionsFieldTag) +
                        BufferWriterExtensions.ComputeLengthPrefixSize(positionsBytes) +
                        positionsBytes;
            }

            return size;
        }

        /// <summary>Writes the message body, reading this sample's positions from the shared buffer by offset.</summary>
        public void WriteMessageTo(ref BufferWriter bufferWriter, NativeArray<Vector3> positionsBuffer)
        {
            var component = Component;
            bufferWriter.WriteTag(ComponentFieldTag);
            bufferWriter.WriteLengthPrefixedMessage(ref component);

            // An empty line (positionCount 0) writes no positions field, which decodes as an empty list.
            if (PositionsCount > 0)
            {
                bufferWriter.WriteTag(PositionsFieldTag);
                bufferWriter.WriteLength(PositionsCount * 3 * BufferWriterExtensions.Fixed32Size);
                for (var i = 0; i < PositionsCount; i++)
                {
                    var position = positionsBuffer[PositionsOffset + i];
                    bufferWriter.WriteFloat(position.x);
                    bufferWriter.WriteFloat(position.y);
                    bufferWriter.WriteFloat(position.z);
                }
            }
        }
    }
}
