using System;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using PLUME.Recorder.Module.Unity.Transform;
using PLUME.Sample;
using PLUME.Sample.Unity;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using Vector3 = PLUME.Sample.Common.Vector3;

namespace PLUME.Recorder.Serializer.Unity
{
    public class TransformUpdatePositionSampleSerializer : TimestampedSampleSerializer<TransformState>
    {
        private readonly PackedSample _packedSample = new()
        {
            Header = new SampleHeader(),
            Payload = new Any { TypeUrl = "fr.liris.plume/" + TransformUpdatePosition.Descriptor.FullName }
        };

        private readonly TransformUpdatePosition _transformUpdatePosition = new()
        {
            Id = new TransformGameObjectIdentifier(),
            LocalPosition = new Vector3()
        };

        // TODO: The serialisation should serialize the Payload (ie. the Any) directly, not the PackedSample. So that the caller is responsible for the header (i.e. timestamp) and we can simplify TimestampedSampleSerializer and SampleSerializer to a single Serializer (StateSerializer).
        public override void Serialize(TransformState unityObjectState, long timestamp, IByteBufferWriter bufferWriter)
        {
            _transformUpdatePosition.Id.TransformId = "TransformId";
            _transformUpdatePosition.Id.GameObjectId = "GameObjectId";
            _transformUpdatePosition.LocalPosition.X = unityObjectState.LocalPosition.x;
            _transformUpdatePosition.LocalPosition.Y = unityObjectState.LocalPosition.y;
            _transformUpdatePosition.LocalPosition.Z = unityObjectState.LocalPosition.z;
            WriteTo(_transformUpdatePosition, timestamp, bufferWriter);
        }

        public override UniTask SerializeAsync(TransformState unityObjectState, long timestamp, IByteBufferWriter bufferWriter)
        {
            // var job = new SerializeTransformsJob();
            // return job.Schedule().ToUniTask(PlayerLoopTiming.Update);

            return UniTask.RunOnThreadPool(() =>
            {
                _transformUpdatePosition.Id.TransformId = "TransformId";
                _transformUpdatePosition.Id.GameObjectId = "GameObjectId";
                _transformUpdatePosition.LocalPosition.X = unityObjectState.LocalPosition.x;
                _transformUpdatePosition.LocalPosition.Y = unityObjectState.LocalPosition.y;
                _transformUpdatePosition.LocalPosition.Z = unityObjectState.LocalPosition.z;
                WriteTo(_transformUpdatePosition, timestamp, bufferWriter);
            });
        }

        public override void SerializeBatch(StateCollection<TransformState> samples, long timestamp,
            IByteBufferWriter bufferWriter)
        {
            foreach (var sample in samples)
            {
                _transformUpdatePosition.Id.TransformId = "TransformId";
                _transformUpdatePosition.Id.GameObjectId = "GameObjectId";
                _transformUpdatePosition.LocalPosition.X = sample.LocalPosition.x;
                _transformUpdatePosition.LocalPosition.Y = sample.LocalPosition.y;
                _transformUpdatePosition.LocalPosition.Z = sample.LocalPosition.z;
                WriteTo(_transformUpdatePosition, timestamp, bufferWriter);
            }
        }

        public override UniTask SerializeBatchAsync(StateCollection<TransformState> samples, long timestamp,
            IByteBufferWriter bufferWriter)
        {
            // TODO: use a job with ProtoBurst?
            
            // var data = new NativeList<TransformUpdateSample>(samples.Count, Allocator.TempJob);
            // data.Resize(samples.Count, NativeArrayOptions.UninitializedMemory);
            // samples.AsSpan().CopyTo(data.AsArray().AsSpan());
            // var job = new SerializeTransformsJob();
            // var jobHandle = job.Schedule();
            // var dependency = data.Dispose(jobHandle);
            // return dependency.ToUniTask(PlayerLoopTiming.Update);
            
            return UniTask.RunOnThreadPool(() =>
            {
                foreach (var sample in samples)
                {
                    _transformUpdatePosition.Id.TransformId = "TransformId";
                    _transformUpdatePosition.Id.GameObjectId = "GameObjectId";
                    _transformUpdatePosition.LocalPosition.X = sample.LocalPosition.x;
                    _transformUpdatePosition.LocalPosition.Y = sample.LocalPosition.y;
                    _transformUpdatePosition.LocalPosition.Z = sample.LocalPosition.z;
                    WriteTo(_transformUpdatePosition, timestamp, bufferWriter);
                }
            });
        }

        private unsafe void WriteTo(TransformUpdatePosition transformUpdatePosition, long timestamp, IByteBufferWriter bufferWriter)
        {
            var size = transformUpdatePosition.CalculateSize();
            var bytes = stackalloc byte[size];
            var span = new Span<byte>(bytes, size);
            transformUpdatePosition.WriteTo(span);
            _packedSample.Header.Time = (ulong)timestamp;
            // TODO: prevent allocations by serializing directly using ProtoBurst
            _packedSample.Payload.Value = ByteString.CopyFrom(span);
            _packedSample.WriteTo(bufferWriter);
        }

        [BurstCompile]
        private struct SerializeTransformsJob : IJob
        {
            public void Execute()
            {
                Debug.Log("Hello from job");
            }
        }
    }
}