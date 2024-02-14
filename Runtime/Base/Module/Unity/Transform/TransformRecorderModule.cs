using System;
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
using Unity.Collections.LowLevel.Unsafe;
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
            CancellationToken forceStopToken)
        {
            Profiler.BeginSample("Poll states");
            var dirtySamples =
                new NativeList<TransformUpdateLocalPositionSample>(RecordedObjects.Count, Allocator.Persistent);
            var dirtySamplesMaxLengthArr = new NativeArray<int>(1, Allocator.Persistent);
            dirtySamplesMaxLengthArr[0] = 0;

            var pollTransformStatesJob = new PollTransformStatesJob
            {
                AlignedIdentifiers = _transformAccessArray.GetAlignedIdentifiers(),
                DirtySamples = dirtySamples.AsParallelWriter(),
                DirtySamplesMaxLength = dirtySamplesMaxLengthArr,
                LastStates = _lastStates
            };

            pollTransformStatesJob.Schedule(_transformAccessArray).Complete();

            var dirtySamplesMaxLength = dirtySamplesMaxLengthArr[0];
            dirtySamplesMaxLengthArr.Dispose();

            Profiler.EndSample();

            if (dirtySamples.Length > 0)
            {
                var requiredDataCapacity = buffer.GetDataCapacity() + dirtySamplesMaxLength;
                var requiredChunksCapacity = buffer.GetChunksCapacity() + dirtySamples.Length;
                
                Profiler.BeginSample("Ensure capacity");
                buffer.EnsureCapacity(requiredDataCapacity, requiredChunksCapacity);
                Profiler.EndSample();
                
                await UniTask.SwitchToThreadPool();
                
                var data = new NativeList<byte>(dirtySamplesMaxLength, Allocator.TempJob);
                var lengths = new NativeList<int>(dirtySamples.Length, Allocator.TempJob);
                
                foreach (var dirtySample in dirtySamples)
                {
                    var prevLength = data.Length;
                    dirtySample.WriteToNoResize(ref data);
                    var newLength = data.Length;
                    lengths.AddNoResize(newLength - prevLength);
                }
                
                buffer.AddSerializedSamplesNoResize(_updatePosSampleTypeUrlIndex, data.AsArray(), lengths.AsArray());
                data.Dispose();
                lengths.Dispose();
                
                await UniTask.SwitchToMainThread();
            }

            dirtySamples.Dispose();
        }

        [BurstCompile]
        private struct PollTransformStatesJob : IJobParallelForTransform

        {
            [ReadOnly] public NativeArray<ObjectIdentifier>.ReadOnly AlignedIdentifiers;

            [WriteOnly] public NativeList<TransformUpdateLocalPositionSample>.ParallelWriter DirtySamples;

            public NativeArray<int> DirtySamplesMaxLength;

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

                    unsafe
                    {
                        Interlocked.Add(ref ((int*)DirtySamplesMaxLength.GetUnsafePtr())[0], sample.ComputeMaxSize());
                    }

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

                    unsafe
                    {
                        Interlocked.Add(ref ((int*)DirtySamplesMaxLength.GetUnsafePtr())[0], sample.ComputeMaxSize());
                    }

                    LastStates[identifier] = state;
                }
            }
        }
    }
}