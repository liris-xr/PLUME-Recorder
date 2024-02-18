using Cysharp.Threading.Tasks;
using PLUME.Core;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.ProtoBurst;
using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Scripting;

namespace PLUME.Base.Module.Unity.Transform
{
    [Preserve]
    internal class
        TransformRecorderModule : ObjectFrameDataRecorderModuleBase<UnityEngine.Transform, TransformFrameData>
    {
        private DynamicTransformAccessArray _transformAccessArray;
        private NativeHashMap<ObjectIdentifier, TransformState> _lastStates;

        protected override void OnCreate(RecorderContext ctx)
        {
            _transformAccessArray = new DynamicTransformAccessArray();
            _lastStates = new NativeHashMap<ObjectIdentifier, TransformState>(1000, Allocator.Persistent);
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
        private JobHandle _pollingJobHandle;

        protected override void OnPreUpdate(Record record, RecorderContext context)
        {
            _dirtySamples =
                new NativeList<TransformUpdateLocalPositionSample>(RecordedObjects.Count, Allocator.Persistent);

            var pollTransformStatesJob = new PollTransformStatesJob
            {
                AlignedIdentifiers = _transformAccessArray.GetAlignedIdentifiers(),
                DirtySamples = _dirtySamples.AsParallelWriter(),
                LastStates = _lastStates // TODO: work on a copy of _lastStates, update after job
            };

            _pollingJobHandle = pollTransformStatesJob.ScheduleReadOnly(_transformAccessArray, 128);
        }

        protected override TransformFrameData OnCollectFrameData(Frame frame)
        {
            _pollingJobHandle.Complete();
            return new TransformFrameData(_dirtySamples);
        }

        [BurstCompile]
        protected override void OnSerializeFrameData(TransformFrameData frameData, Frame frame,
            FrameDataWriter frameDataWriter)
        {
            // TODO: serialize in parallel and put everything inside a DataChunks
            frameDataWriter.Write(frameData.DirtySamples);
        }

        protected override void OnDisposeFrameData(TransformFrameData frameData, Frame frame)
        {
            frameData.Dispose();
        }

        protected override void OnForceStopRecording(Record record, RecorderContext recorderContext)
        {
            _pollingJobHandle.Complete();
        }

        protected override async UniTask OnStopRecording(Record record, RecorderContext recorderContext)
        {
            await UniTask.WaitUntil(() => _pollingJobHandle.IsCompleted);
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
                    LastStates[identifier] = state;
                }
            }
        }
    }
}