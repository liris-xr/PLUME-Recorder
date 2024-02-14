using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using PLUME.Core;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.ProtoBurst;
using PLUME.Sample.Unity;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

namespace PLUME.Base.Module.Unity.Transform
{
    [Preserve]
    internal class TransformRecorderModule : ObjectFrameDataRecorderModuleBase<UnityEngine.Transform>
    {
        private DynamicTransformAccessArray _transformAccessArray;
        private NativeHashMap<ObjectIdentifier, TransformState> _lastStates;
        private SampleTypeUrlIndex _updatePosSampleTypeUrlIndex;

        protected override void OnCreate(RecorderContext ctx)
        {
            _transformAccessArray = new DynamicTransformAccessArray();
            _lastStates = new NativeHashMap<ObjectIdentifier, TransformState>(1000, Allocator.Persistent);

            // TODO: register type urls when loading assemblies
            _updatePosSampleTypeUrlIndex =
                ctx.SampleTypeUrlRegistry.GetOrCreateTypeUrlIndex("fr.liris.plume",
                    TransformUpdateLocalPosition.Descriptor);
        }

        protected override void OnDestroy(RecorderContext ctx)
        {
            if (_transformAccessArray.IsCreated)
            {
                _transformAccessArray.Dispose();
            }

            if (_lastStates.IsCreated)
            {
                _lastStates.Dispose();
            }
        }

        protected override void OnStartRecordingObject(ObjectSafeRef<UnityEngine.Transform> objSafeRef,
            bool markCreated)
        {
            _transformAccessArray.TryAdd(objSafeRef);
            _lastStates[objSafeRef.Identifier] = TransformState.Null;
        }

        protected override void OnStopRecordingObject(ObjectSafeRef<UnityEngine.Transform> objSafeRef,
            bool markDestroyed)
        {
            _transformAccessArray.RemoveSwapBack(objSafeRef);
            _lastStates.Remove(objSafeRef.Identifier);
        }

        protected override void OnReset(RecorderContext ctx)
        {
            _transformAccessArray.Clear();
            _lastStates.Clear();
        }

        [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        protected override async UniTask OnRecordFrameData(SerializedSamplesBuffer buffer,
            CancellationToken cancellationToken)
        {
            Profiler.BeginSample("Poll states");
            var dirtySamples =
                new NativeList<TransformUpdateLocalPositionSample>(RecordedObjects.Count, Allocator.Persistent);

            var pollTransformStatesJob = new PollTransformStatesJob
            {
                AlignedIdentifiers = _transformAccessArray.GetAlignedIdentifiers(),
                DirtySamples = dirtySamples.AsParallelWriter(),
                LastStates = _lastStates
            };

            pollTransformStatesJob.Schedule(_transformAccessArray).Complete();
            Profiler.EndSample();
            
            if (dirtySamples.Length == 0)
            {
                dirtySamples.Dispose();
                return;
            }

            var maxCapacity = TransformUpdateLocalPositionSample.MaxSize * dirtySamples.Length;
            var data = new NativeList<byte>(maxCapacity, Allocator.Persistent);
            var lengths = new NativeList<int>(dirtySamples.Length, Allocator.Persistent);
            
            var serializeJob = new SerializeDirtySamplesJob
            {
                Data = data,
                Lengths = lengths,
                DirtySamples = dirtySamples
            };
            var serializeJobHandle = serializeJob.Schedule();
            await serializeJobHandle.WaitAsync(PlayerLoopTiming.Update, cancellationToken);
            
            dirtySamples.Dispose();
            buffer.AddSerializedSamples(_updatePosSampleTypeUrlIndex, data.AsArray(), lengths.AsArray());
            data.Dispose();
            lengths.Dispose();
        }

        [BurstCompile]
        private struct SerializeDirtySamplesJob : IJob
        {
            [ReadOnly] public NativeList<TransformUpdateLocalPositionSample> DirtySamples;
            public NativeList<byte> Data;
            public NativeList<int> Lengths;

            [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
            public void Execute()
            {
                foreach (var dirtySample in DirtySamples)
                {
                    var prevLength = Data.Length;
                    dirtySample.WriteTo(ref Data);
                    var newLength = Data.Length;
                    Lengths.Add(newLength - prevLength);
                }
            }
        }

        [BurstCompile]
        private struct PollTransformStatesJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<ObjectIdentifier>.ReadOnly AlignedIdentifiers;

            [WriteOnly] public NativeList<TransformUpdateLocalPositionSample>.ParallelWriter DirtySamples;

            [NativeDisableParallelForRestriction] public NativeHashMap<ObjectIdentifier, TransformState> LastStates;

            public void Execute(int index, TransformAccess transform)
            {
                var identifier = AlignedIdentifiers[index];
                var lastSample = LastStates[identifier];

                if (lastSample.IsNull)
                {
                    var state = new TransformState
                    {
                        Identifier = identifier,
                        LocalPosition = transform.localPosition,
                        LocalRotation = transform.localRotation,
                        LocalScale = transform.localScale
                    };

                    var sample = new TransformUpdateLocalPositionSample
                    {
                        LocalPosition = new Vector3Sample
                        {
                            X = state.LocalPosition.x,
                            Y = state.LocalPosition.y,
                            Z = state.LocalPosition.z
                        }
                    };

                    DirtySamples.AddNoResize(sample);
                    LastStates[identifier] = state;
                }
                else
                {
                    var localPosition = transform.localPosition;
                    var localRotation = transform.localRotation;
                    var localScale = transform.localScale;

                    // var isLocalPositionDirty = lastSample.LocalPosition != localPosition;
                    // var isLocalRotationDirty = lastSample.LocalRotation != localRotation;
                    // var isLocalScaleDirty = lastSample.LocalScale != localScale;
                    //
                    // if (!isLocalPositionDirty && !isLocalRotationDirty && !isLocalScaleDirty)
                    //     return;

                    var state = new TransformState
                    {
                        Identifier = identifier,
                        LocalPosition = localPosition,
                        LocalRotation = localRotation,
                        LocalScale = localScale
                    };

                    var sample = new TransformUpdateLocalPositionSample
                    {
                        LocalPosition = new Vector3Sample
                        {
                            X = state.LocalPosition.x,
                            Y = state.LocalPosition.y,
                            Z = state.LocalPosition.z
                        }
                    };

                    DirtySamples.AddNoResize(sample);
                    LastStates[identifier] = state;
                }
            }
        }
    }
}