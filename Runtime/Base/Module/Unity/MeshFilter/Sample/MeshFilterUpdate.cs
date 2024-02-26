using PLUME.Sample.ProtoBurst;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;

namespace PLUME.Base.Module.Unity.MeshFilter.Sample
{
    [BurstCompile]
    public struct MeshFilterUpdate : IProtoBurstMessage
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.unity.MeshFilterUpdate";

        private static readonly uint IdentifierFieldTag = WireFormat.MakeTag(1, WireFormat.WireType.LengthDelimited);
        private static readonly uint MeshIdentifierFieldTag = WireFormat.MakeTag(2, WireFormat.WireType.LengthDelimited);
        private static readonly uint SharedMeshFieldTag = WireFormat.MakeTag(3, WireFormat.WireType.LengthDelimited);

        private ComponentIdentifier _identifier;
        
        private bool _hasMeshIdentifierField;
        private AssetIdentifier _meshIdentifier;
        
        private bool _hasSharedMeshIdentifierField;
        private AssetIdentifier _sharedMeshIdentifier;

        public MeshFilterUpdate(ComponentIdentifier identifier)
        {
            _identifier = identifier;
            
            _hasMeshIdentifierField = false;
            _meshIdentifier = default;
            
            _hasSharedMeshIdentifierField = false;
            _sharedMeshIdentifier = default;
        }

        public int ComputeSize()
        {
            var size = BufferWriterExtensions.ComputeTagSize(IdentifierFieldTag) +
                    BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref _identifier);
            
            if (_hasMeshIdentifierField)
            {
                size += BufferWriterExtensions.ComputeTagSize(MeshIdentifierFieldTag) +
                        BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref _meshIdentifier);
            }
            
            if (_hasSharedMeshIdentifierField)
            {
                size += BufferWriterExtensions.ComputeTagSize(SharedMeshFieldTag) +
                        BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref _sharedMeshIdentifier);
            }
            
            return size;
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            bufferWriter.WriteTag(IdentifierFieldTag);
            bufferWriter.WriteLengthPrefixedMessage(ref _identifier);
            
            if (_hasMeshIdentifierField)
            {
                bufferWriter.WriteTag(MeshIdentifierFieldTag);
                bufferWriter.WriteLengthPrefixedMessage(ref _meshIdentifier);
            }
            
            if (_hasSharedMeshIdentifierField)
            {
                bufferWriter.WriteTag(SharedMeshFieldTag);
                bufferWriter.WriteLengthPrefixedMessage(ref _sharedMeshIdentifier);
            }
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }
        
        public void SetMeshIdentifier(AssetIdentifier meshIdentifier)
        {
            _hasMeshIdentifierField = true;
            _meshIdentifier = meshIdentifier;
        }
        
        public void SetSharedMeshIdentifier(AssetIdentifier sharedMesh)
        {
            _hasSharedMeshIdentifierField = true;
            _sharedMeshIdentifier = sharedMesh;
        }
    }
}