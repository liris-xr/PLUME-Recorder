using System;
using PLUME.Core.Object;
using ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;
using Guid = PLUME.Core.Guid;

namespace PLUME.Sample.ProtoBurst.Unity
{
    [BurstCompile]
    public readonly struct GameObjectIdentifier : IObjectIdentifier, IProtoBurstMessage,
        IEquatable<GameObjectIdentifier>
    {
        public static readonly FixedString128Bytes TypeUrl = "fr.liris.plume/plume.sample.unity.GameObjectIdentifier";

        public static GameObjectIdentifier Null { get; } = new(0, Guid.Null, 0, Guid.Null, SceneIdentifier.Null);

        private static readonly uint GuidFieldTag =
            WireFormat.MakeTag(Sample.Unity.GameObjectIdentifier.GuidFieldNumber, WireFormat.WireType.LengthDelimited);

        private static readonly uint TransformGuidFieldTag =
            WireFormat.MakeTag(Sample.Unity.GameObjectIdentifier.TransformGuidFieldNumber,
                WireFormat.WireType.LengthDelimited);

        private static readonly uint SceneFieldTag =
            WireFormat.MakeTag(Sample.Unity.GameObjectIdentifier.SceneFieldNumber, WireFormat.WireType.LengthDelimited);

        public readonly int RuntimeId;
        public readonly Guid Guid;

        public readonly int TransformRuntimeId;
        public readonly Guid TransformGuid;
        
        public readonly SceneIdentifier Scene;

        public GameObjectIdentifier(int runtimeId, Guid guid, int transformRuntimeId, Guid transformGuid, SceneIdentifier scene)
        {
            RuntimeId = runtimeId;
            Guid = guid;
            TransformRuntimeId = transformRuntimeId;
            TransformGuid = transformGuid;
            Scene = scene;
        }

        public int ComputeSize()
        {
            var scene = Scene;
            
            return BufferWriterExtensions.ComputeTagSize(GuidFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixSize(Guid.Size) +
                   Guid.Size +
                   BufferWriterExtensions.ComputeTagSize(TransformGuidFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixSize(Guid.Size) +
                   Guid.Size +
                   BufferWriterExtensions.ComputeTagSize(SceneFieldTag) +
                   BufferWriterExtensions.ComputeLengthPrefixedMessageSize(ref scene);
        }

        public void WriteTo(ref BufferWriter bufferWriter)
        {
            var guid = Guid;
            bufferWriter.WriteTag(GuidFieldTag);
            bufferWriter.WriteLength(Guid.Size);
            guid.WriteTo(ref bufferWriter);

            var transformGuid = TransformGuid;
            bufferWriter.WriteTag(TransformGuidFieldTag);
            bufferWriter.WriteLength(Guid.Size);
            transformGuid.WriteTo(ref bufferWriter);
            
            var scene = Scene;
            bufferWriter.WriteTag(SceneFieldTag);
            bufferWriter.WriteLengthPrefixedMessage(ref scene);
        }

        public SampleTypeUrl GetTypeUrl(Allocator allocator)
        {
            return SampleTypeUrl.Alloc(TypeUrl, allocator);
        }

        public bool Equals(GameObjectIdentifier other)
        {
            return RuntimeId.Equals(other.RuntimeId);
        }

        public override int GetHashCode()
        {
            return RuntimeId.GetHashCode();
        }
    }
}