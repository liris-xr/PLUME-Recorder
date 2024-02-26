using PLUME.Sample.ProtoBurst;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace PLUME.Base.Module.Unity.Transform.Sample
{
    [BurstCompile]
    public struct TransformUpdate : IProtoBurstMessage
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.unity.TransformUpdate";

        private static readonly uint IdentifierFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.LengthDelimited);
        private static readonly uint LocalPositionFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);
        private static readonly uint LocalRotationFieldTag = WireFormat.MakeTag(3, WireFormat.WireType.LengthDelimited);
        private static readonly uint LocalScaleFieldTag = WireFormat.MakeTag(4, WireFormat.WireType.LengthDelimited);

        private TransformGameObjectIdentifier _identifier;

        private bool _hasLocalPosition;
        private Vector3 _localPosition;

        private bool _hasLocalRotation;
        private Quaternion _localRotation;

        private bool _hasLocalScale;
        private Vector3 _localScale;

        public TransformUpdate(TransformGameObjectIdentifier identifier)
        {
            _identifier = identifier;

            _hasLocalPosition = false;
            _localPosition = default;
            _hasLocalRotation = false;
            _localRotation = default;
            _hasLocalScale = false;
            _localScale = default;
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            bufferWriter.WriteTag(IdentifierFieldTag);
            bufferWriter.WriteLengthPrefixedMessage(ref _identifier);

            if (_hasLocalPosition)
            {
                bufferWriter.WriteTag(LocalPositionFieldTag);
                bufferWriter.WriteLengthPrefixedMessage(ref _localPosition);
            }

            if (_hasLocalRotation)
            {
                bufferWriter.WriteTag(LocalRotationFieldTag);
                bufferWriter.WriteLengthPrefixedMessage(ref _localRotation);
            }

            if (_hasLocalScale)
            {
                bufferWriter.WriteTag(LocalScaleFieldTag);
                bufferWriter.WriteLengthPrefixedMessage(ref _localScale);
            }
        }

        public int ComputeSize()
        {
            var size = 0;

            size += BufferWriterExtensions.ComputeTagSize(IdentifierFieldTag) +
                    BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref _identifier);

            if (_hasLocalPosition)
            {
                size += BufferWriterExtensions.ComputeTagSize(LocalPositionFieldTag) +
                        BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref _localPosition);
            }

            if (_hasLocalRotation)
            {
                size += BufferWriterExtensions.ComputeTagSize(LocalRotationFieldTag) +
                        BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref _localRotation);
            }

            if (_hasLocalScale)
            {
                size += BufferWriterExtensions.ComputeTagSize(LocalScaleFieldTag) +
                        BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref _localScale);
            }

            return size;
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public void SetLocalPosition(float3 localPosition)
        {
            _hasLocalPosition = true;
            _localPosition = new Vector3(localPosition);
        }

        public void SetLocalRotation(quaternion localRotation)
        {
            _hasLocalRotation = true;
            _localRotation = new Quaternion(localRotation);
        }

        public void SetLocalScale(float3 localScale)
        {
            _hasLocalScale = true;
            _localScale = new Vector3(localScale);
        }
    }
}