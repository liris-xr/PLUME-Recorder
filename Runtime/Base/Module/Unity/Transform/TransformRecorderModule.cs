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
using Unity.Jobs;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

namespace PLUME.Base.Module.Unity.Transform
{
    [Preserve]
    internal class TransformRecorderModule : ObjectFrameDataRecorderModuleBase<UnityEngine.Transform, TransformFrameData>
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

        private NativeList<TransformUpdateLocalPositionSample> _dirtySamples;
        private NativeArray<int> _dirtySamplesMaxLengthArray;
        private JobHandle _pollingJobHandle;
        
        protected override void OnPreUpdate(RecordContext recordContext, RecorderContext context)
        {
            Profiler.BeginSample("Create native collections");
            _dirtySamples = new NativeList<TransformUpdateLocalPositionSample>(RecordedObjects.Count, Allocator.Persistent);
            _dirtySamplesMaxLengthArray = new NativeArray<int>(1, Allocator.Persistent);
            Profiler.EndSample();
            
            Profiler.BeginSample("Create job");
            var pollTransformStatesJob = new PollTransformStatesJob
            {
                AlignedIdentifiers = _transformAccessArray.GetAlignedIdentifiers(),
                DirtySamples = _dirtySamples.AsParallelWriter(),
                DirtySamplesMaxLength = _dirtySamplesMaxLengthArray,
                // TODO: work on a copy of _lastStates, update after job
                LastStates = _lastStates
            };
            Profiler.EndSample();
            
            Profiler.BeginSample("Schedule job");
            _pollingJobHandle = pollTransformStatesJob.ScheduleReadOnly(_transformAccessArray, 128);
            Profiler.EndSample();
        }

        // TODO: start job earlier in the frame
        
        protected override TransformFrameData CollectFrameData()
        {
            Profiler.BeginSample("OnCollectFrameData");
            
            Profiler.BeginSample("Wait for job");
            _pollingJobHandle.Complete();
            Profiler.EndSample();
            
            var dirtySamplesMaxLength = _dirtySamplesMaxLengthArray[0];
            _dirtySamplesMaxLengthArray.Dispose();
            
            var transformFrameData = new TransformFrameData(_dirtySamples, dirtySamplesMaxLength);
            Profiler.EndSample();
            return transformFrameData;
        }

        protected override void SerializeFrameData(TransformFrameData frameData, SerializedSamplesBuffer buffer)
        {
            buffer.EnsureCapacity(frameData.DirtySamplesMaxLength, frameData.DirtySamples.Length);
            
            var data = new NativeList<byte>(frameData.DirtySamplesMaxLength, Allocator.TempJob);
            var lengths = new NativeList<int>(frameData.DirtySamples.Length, Allocator.TempJob);
            
            foreach (var dirtySample in frameData.DirtySamples)
            {
                var prevLength = data.Length;
                dirtySample.WriteToNoResize(ref data);
                var newLength = data.Length;
                lengths.AddNoResize(newLength - prevLength);
            }
            
            buffer.AddSerializedSamplesNoResize(_updatePosSampleTypeUrlIndex, data.AsArray(), lengths.AsArray());
            data.Dispose();
            lengths.Dispose();
        }

        protected override void DisposeFrameData(TransformFrameData frameData)
        {
            frameData.Dispose();
        }

        protected override void OnForceStop(RecordContext recordContext, RecorderContext recorderContext)
        {
            _pollingJobHandle.Complete();
        }

        protected override async UniTask OnStop(RecordContext recordContext, RecorderContext recorderContext)
        {
            await UniTask.WaitUntil(() => _pollingJobHandle.IsCompleted);
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

                    // TODO
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