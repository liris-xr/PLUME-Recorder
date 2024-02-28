using System.Runtime.CompilerServices;
using PLUME.Core.Object;
using PLUME.Sample.ProtoBurst.Common;
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
        private static readonly uint ParentIdentifierFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);
        private static readonly uint SiblingIndexFieldTag = WireFormat.MakeTag(3, WireFormat.WireType.VarInt);
        private static readonly uint LocalPositionFieldTag = WireFormat.MakeTag(4, WireFormat.WireType.LengthDelimited);
        private static readonly uint LocalRotationFieldTag = WireFormat.MakeTag(5, WireFormat.WireType.LengthDelimited);
        private static readonly uint LocalScaleFieldTag = WireFormat.MakeTag(6, WireFormat.WireType.LengthDelimited);

        private ComponentIdentifier _identifier;

        private bool _hasParentTransformIdField;
        private ComponentIdentifier _parentTransformId;
        
        private bool _hasSiblingIndexField;
        private int _siblingIndex;

        private bool _hasLocalPositionField;
        private Vector3 _localPosition;

        private bool _hasLocalRotationField;
        private Quaternion _localRotation;

        private bool _hasLocalScaleField;
        private Vector3 _localScale;

        public TransformUpdate(ComponentIdentifier identifier)
        {
            _identifier = identifier;
            
            _hasParentTransformIdField = false;
            _parentTransformId = default;
            
            _hasSiblingIndexField = false;
            _siblingIndex = default;
            
            _hasLocalPositionField = false;
            _localPosition = default;
            
            _hasLocalRotationField = false;
            _localRotation = default;
            
            _hasLocalScaleField = false;
            _localScale = default;
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            bufferWriter.WriteTag(IdentifierFieldTag);
            bufferWriter.WriteLengthPrefixedMessage(ref _identifier);

            if (_hasParentTransformIdField)
            {
                bufferWriter.WriteTag(ParentIdentifierFieldTag);
                bufferWriter.WriteLengthPrefixedMessage(ref _parentTransformId);
            }
            
            if (_hasSiblingIndexField)
            {
                bufferWriter.WriteTag(SiblingIndexFieldTag);
                bufferWriter.WriteInt32(_siblingIndex);
            }
            
            if (_hasLocalPositionField)
            {
                bufferWriter.WriteTag(LocalPositionFieldTag);
                bufferWriter.WriteLengthPrefixedMessage(ref _localPosition);
            }

            if (_hasLocalRotationField)
            {
                bufferWriter.WriteTag(LocalRotationFieldTag);
                bufferWriter.WriteLengthPrefixedMessage(ref _localRotation);
            }

            if (_hasLocalScaleField)
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

            if (_hasParentTransformIdField)
            {
                size += BufferWriterExtensions.ComputeTagSize(ParentIdentifierFieldTag) +
                        BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref _parentTransformId);
            }
            
            if (_hasSiblingIndexField)
            {
                size += BufferWriterExtensions.ComputeTagSize(SiblingIndexFieldTag) +
                        BufferWriterExtensions.ComputeInt32Size(_siblingIndex);
            }
            
            if (_hasLocalPositionField)
            {
                size += BufferWriterExtensions.ComputeTagSize(LocalPositionFieldTag) +
                        BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref _localPosition);
            }

            if (_hasLocalRotationField)
            {
                size += BufferWriterExtensions.ComputeTagSize(LocalRotationFieldTag) +
                        BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref _localRotation);
            }

            if (_hasLocalScaleField)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetParent(ComponentIdentifier parent)
        {
            _hasParentTransformIdField = true;
            _parentTransformId = parent;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSiblingIndex(int siblingIndex)
        {
            _hasSiblingIndexField = true;
            _siblingIndex = siblingIndex;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLocalPosition(float3 localPosition)
        {
            _hasLocalPositionField = true;
            _localPosition = new Vector3(localPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLocalRotation(quaternion localRotation)
        {
            _hasLocalRotationField = true;
            _localRotation = new Quaternion(localRotation);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLocalScale(float3 localScale)
        {
            _hasLocalScaleField = true;
            _localScale = new Vector3(localScale);
        }
    }
}