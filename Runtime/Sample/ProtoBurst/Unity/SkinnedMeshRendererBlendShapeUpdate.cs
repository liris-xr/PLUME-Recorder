using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Sample.ProtoBurst.Unity
{
    /// <summary>
    /// Blittable descriptor of a skinned mesh renderer blend shape update. The weights themselves live in a shared
    /// per-frame <see cref="Unity.Collections.NativeList{T}"/> owned by the frame data; this struct only references
    /// them by <see cref="WeightsOffset"/> / <see cref="WeightsCount"/>. This keeps the sample tiny (no inline
    /// weight buffer, no fixed capacity) and lets the frame data serialize any number of weights with no managed
    /// allocation and no per-weight object.
    /// </summary>
    [BurstCompile]
    public struct SkinnedMeshRendererBlendShapeUpdate
    {
        public static readonly FixedString128Bytes TypeUrl =
            "fr.liris.plume/plume.sample.unity.SkinnedMeshRendererBlendShapeUpdate";

        public static readonly uint ComponentFieldTag =
            WireFormat.MakeTag(Sample.Unity.SkinnedMeshRendererBlendShapeUpdate.ComponentFieldNumber,
                WireFormat.WireType.LengthDelimited);

        public static readonly uint WeightsFieldTag =
            WireFormat.MakeTag(Sample.Unity.SkinnedMeshRendererBlendShapeUpdate.WeightsFieldNumber,
                WireFormat.WireType.LengthDelimited);

        public ComponentIdentifier Component;
        public int WeightsOffset;
        public int WeightsCount;

        /// <summary>Serialized size of the message body (component + packed weights), excluding the Any envelope.</summary>
        public int ComputeMessageSize()
        {
            var component = Component;
            var size = BufferWriterExtensions.ComputeTagSize(ComponentFieldTag) +
                       BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref component);

            if (WeightsCount > 0)
            {
                var weightsBytes = WeightsCount * BufferWriterExtensions.Fixed32Size;
                size += BufferWriterExtensions.ComputeTagSize(WeightsFieldTag) +
                        BufferWriterExtensions.ComputeLengthPrefixSize(weightsBytes) +
                        weightsBytes;
            }

            return size;
        }

        /// <summary>Writes the message body, reading this sample's weights from the shared buffer by offset.</summary>
        public void WriteMessageTo(ref BufferWriter bufferWriter, NativeArray<float> weightsBuffer)
        {
            var component = Component;
            bufferWriter.WriteTag(ComponentFieldTag);
            bufferWriter.WriteLengthPrefixedMessage(ref component);

            if (WeightsCount > 0)
            {
                bufferWriter.WriteTag(WeightsFieldTag);
                bufferWriter.WriteLength(WeightsCount * BufferWriterExtensions.Fixed32Size);
                for (var i = 0; i < WeightsCount; i++)
                {
                    bufferWriter.WriteFloat(weightsBuffer[WeightsOffset + i]);
                }
            }
        }
    }
}
