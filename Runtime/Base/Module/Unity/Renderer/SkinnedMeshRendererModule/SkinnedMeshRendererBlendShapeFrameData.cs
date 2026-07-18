using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.ProtoBurst.Unity;
using ProtoBurst;
using ProtoBurst.Message;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Collections;
using Frame = PLUME.Sample.ProtoBurst.Unity.Frame;

namespace PLUME.Base.Module.Unity.Renderer.SkinnedMeshRendererModule
{
    /// <summary>
    /// Native, allocation-free frame data for the decoupled blend shape stream. Holds the per-frame update
    /// descriptors and a single shared weights buffer they index into, so any number of weights is serialized
    /// without a fixed capacity and without managed allocation.
    /// </summary>
    public struct SkinnedMeshRendererBlendShapeFrameData : IFrameData
    {
        private NativeList<SkinnedMeshRendererBlendShapeUpdate> _updateSamples;
        private NativeList<float> _weights;

        public SkinnedMeshRendererBlendShapeFrameData(
            NativeList<SkinnedMeshRendererBlendShapeUpdate> updateSamples, NativeList<float> weights)
        {
            _updateSamples = updateSamples;
            _weights = weights;
        }

        public void Serialize(FrameDataWriter frameDataWriter)
        {
            if (!_updateSamples.IsCreated || _updateSamples.Length == 0)
                return;

            var typeUrl = SampleTypeUrl.Alloc(SkinnedMeshRendererBlendShapeUpdate.TypeUrl, Allocator.Persistent);
            var typeUrlBytes = typeUrl.AsArray();
            var weights = _weights.AsArray();

            // First pass: exact serialized size (BufferWriter writes without resizing and throws on overflow).
            var totalSize = 0;
            for (var i = 0; i < _updateSamples.Length; i++)
            {
                var messageSize = _updateSamples[i].ComputeMessageSize();
                var anySize = Any.ComputeSize(typeUrlBytes.Length, messageSize);
                totalSize += BufferWriterExtensions.ComputeTagSize(Frame.DataFieldTag) +
                             BufferWriterExtensions.ComputeLengthPrefixSize(anySize) + anySize;
            }

            var rawBytes = new NativeList<byte>(totalSize, Allocator.Persistent);
            var bufferWriter = new BufferWriter(rawBytes);

            // Second pass: write each sample as a Frame "data" entry wrapping an Any(SkinnedMeshRendererBlendShapeUpdate).
            for (var i = 0; i < _updateSamples.Length; i++)
            {
                var update = _updateSamples[i];
                var messageSize = update.ComputeMessageSize();

                bufferWriter.WriteTag(Frame.DataFieldTag);
                bufferWriter.WriteLength(Any.ComputeSize(typeUrlBytes.Length, messageSize));

                bufferWriter.WriteTag(Any.TypeUrlTag);
                bufferWriter.WriteLengthPrefixedBytes(ref typeUrlBytes);
                bufferWriter.WriteTag(Any.ValueTag);
                bufferWriter.WriteLength(messageSize);
                update.WriteMessageTo(ref bufferWriter, weights);
            }

            frameDataWriter.WriteRaw(rawBytes);

            rawBytes.Dispose();
            typeUrl.Dispose();
        }

        public void Dispose()
        {
            if (_updateSamples.IsCreated)
                _updateSamples.Dispose();
            if (_weights.IsCreated)
                _weights.Dispose();
        }
    }
}
