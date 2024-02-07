using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace PLUME.Recorder.Module.Unity.Transform
{
    public class TransformRecorderModule : UnityObjectRecorderModule<UnityEngine.Transform>
    {
        private readonly DynamicTransformAccessArray _transformAccessArray = new();
        private NativeHashMap<ObjectIdentifier, TransformState> _lastStates;

        protected override void OnCreate()
        {
            base.OnCreate();
            _lastStates = new NativeHashMap<ObjectIdentifier, TransformState>(1000, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_lastStates.IsCreated)
            {
                _lastStates.Dispose();
            }
        }

        protected override void OnStartRecording(ObjectSafeRef<UnityEngine.Transform> objSafeRef, bool markCreated)
        {
            base.OnStartRecording(objSafeRef, markCreated);
            _transformAccessArray.TryAdd(objSafeRef);
            _lastStates[objSafeRef.ObjectIdentifier] = TransformState.Null;
        }

        protected override void OnStopRecording(ObjectSafeRef<UnityEngine.Transform> objSafeRef, bool markDestroyed)
        {
            base.OnStopRecording(objSafeRef, markDestroyed);
            _transformAccessArray.RemoveSwapBack(objSafeRef);
            _lastStates.Remove(objSafeRef.ObjectIdentifier);
        }

        protected override void OnReset()
        {
            base.OnReset();
            _transformAccessArray.Clear();
            _lastStates.Clear();
        }

        protected override void OnRecordFrame(FrameData frameData)
        {
            base.OnRecordFrame(frameData);

            foreach (var transformSafeRef in CreatedObjects)
            {
                frameData.AddSample(new TransformCreatedState(transformSafeRef.GetInstanceId()));
            }

            foreach (var transformSafeRef in DestroyedObjects)
            {
                frameData.AddSample(new TransformDestroyedState(transformSafeRef.GetInstanceId()));
            }

            var dirtySamples = new NativeList<TransformState>(Allocator.TempJob);
            dirtySamples.SetCapacity(_transformAccessArray.Length);

            var pollTransformStatesJob = new PollTransformStatesJob
            {
                AlignedIdentifiers = _transformAccessArray.GetAlignedIdentifiers(),
                DirtySamples = dirtySamples.AsParallelWriter(),
                LastSamples = _lastStates
            };

            var pollTransformStatesJobHandle = pollTransformStatesJob.ScheduleReadOnly(_transformAccessArray, 64);
            pollTransformStatesJobHandle.Complete();
            frameData.AddSamples(dirtySamples.AsArray().AsReadOnlySpan());
            dirtySamples.Dispose();
        }

        [BurstCompile]
        private struct PollTransformStatesJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<ObjectIdentifier>.ReadOnly AlignedIdentifiers;

            [WriteOnly] public NativeList<TransformState>.ParallelWriter DirtySamples;

            [NativeDisableParallelForRestriction] public NativeHashMap<ObjectIdentifier, TransformState> LastSamples;

            public void Execute(int index, TransformAccess transform)
            {
                var identifier = AlignedIdentifiers[index];
                var lastSample = LastSamples[identifier];

                if (lastSample.IsNull)
                {
                    var sample = new TransformState
                    {
                        Identifier = identifier,
                        LocalPosition = transform.localPosition,
                        LocalRotation = transform.localRotation,
                        LocalScale = transform.localScale
                    };

                    Debug.Log("Adding new sample");
                    DirtySamples.AddNoResize(sample);
                    LastSamples[identifier] = sample;
                }
                else
                {
                    var localPosition = transform.localPosition;
                    var localRotation = transform.localRotation;
                    var localScale = transform.localScale;

                    var isLocalPositionDirty = lastSample.LocalPosition != localPosition;
                    var isLocalRotationDirty = lastSample.LocalRotation != localRotation;
                    var isLocalScaleDirty = lastSample.LocalScale != localScale;

                    if (!isLocalPositionDirty && !isLocalRotationDirty && !isLocalScaleDirty)
                        return;

                    var sample = new TransformState
                    {
                        Identifier = identifier,
                        LocalPosition = transform.localPosition,
                        LocalRotation = transform.localRotation,
                        LocalScale = transform.localScale
                    };

                    Debug.Log("Adding dirty sample");
                    DirtySamples.AddNoResize(sample);
                    LastSamples[identifier] = sample;
                }
            }
        }
    }
}