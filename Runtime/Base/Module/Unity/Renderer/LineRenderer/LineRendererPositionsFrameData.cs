using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.ProtoBurst.Unity;
using ProtoBurst;
using ProtoBurst.Message;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Collections;
using UnityEngine;
using Frame = PLUME.Sample.ProtoBurst.Unity.Frame;

namespace PLUME.Base.Module.Unity.Renderer.LineRenderer
{
    /// <summary>
    /// Native, allocation-free frame data for the line renderer positions stream. Holds the per-frame update
    /// descriptors and a single shared positions buffer they index into.
    /// </summary>
    public struct LineRendererPositionsFrameData : IFrameData
    {
        private NativeList<LineRendererPositionsUpdate> _updateSamples;
        private NativeList<Vector3> _positions;

        public LineRendererPositionsFrameData(NativeList<LineRendererPositionsUpdate> updateSamples,
            NativeList<Vector3> positions)
        {
            _updateSamples = updateSamples;
            _positions = positions;
        }

        public void Serialize(FrameDataWriter frameDataWriter)
        {
            if (!_updateSamples.IsCreated || _updateSamples.Length == 0)
                return;

            var typeUrl = SampleTypeUrl.Alloc(LineRendererPositionsUpdate.TypeUrl, Allocator.Persistent);
            var typeUrlBytes = typeUrl.AsArray();
            var positions = _positions.AsArray();

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

            // Second pass: write each sample as a Frame "data" entry wrapping an Any(LineRendererPositionsUpdate).
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
                update.WriteMessageTo(ref bufferWriter, positions);
            }

            frameDataWriter.WriteRaw(rawBytes);

            rawBytes.Dispose();
            typeUrl.Dispose();
        }

        public void Dispose()
        {
            if (_updateSamples.IsCreated)
                _updateSamples.Dispose();
            if (_positions.IsCreated)
                _positions.Dispose();
        }
    }
}
