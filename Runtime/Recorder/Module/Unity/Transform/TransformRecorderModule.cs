using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using PLUME.Sample.Unity;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Pool;
using Vector3 = PLUME.Sample.Common.Vector3;

namespace PLUME.Recorder.Module.Unity.Transform
{
    public class TransformRecorderModule : UnityObjectFrameRecorderModuleAsync<UnityEngine.Transform>
    {
        private readonly DynamicTransformAccessArray _transformAccessArray = new();
        private NativeHashMap<ObjectIdentifier, TransformState> _lastStates;
        private SampleTypeUrlIndex _transformUpdatePositionSampleTypeUrlIndex;

        private readonly ObjectPool<TransformUpdatePosition> _transformUpdatePositionPool = new(() => new TransformUpdatePosition
        {
            Id = new TransformGameObjectIdentifier(),
            LocalPosition = new Vector3(),
            WorldPosition = new Vector3()
        });

        protected override void OnCreate()
        {
            _lastStates = new NativeHashMap<ObjectIdentifier, TransformState>(1000, Allocator.Persistent);
            _transformUpdatePositionSampleTypeUrlIndex =
                SampleTypeUrlManager.GetTypeUrlIndex("fr.liris.plume/" + TransformUpdatePosition.Descriptor.FullName);
        }

        protected override void OnDestroy()
        {
            if (_lastStates.IsCreated)
            {
                _lastStates.Dispose();
            }
        }

        protected override void OnStartRecording(ObjectSafeRef<UnityEngine.Transform> objSafeRef, bool markCreated)
        {
            _transformAccessArray.TryAdd(objSafeRef);
            _lastStates[objSafeRef.ObjectIdentifier] = TransformState.Null;
        }

        protected override void OnStopRecording(ObjectSafeRef<UnityEngine.Transform> objSafeRef, bool markDestroyed)
        {
            _transformAccessArray.RemoveSwapBack(objSafeRef);
            _lastStates.Remove(objSafeRef.ObjectIdentifier);
        }

        protected override void OnReset()
        {
            _transformAccessArray.Clear();
            _lastStates.Clear();
        }

        [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        protected override async UniTask OnRecordFrame(FrameDataBuffer buffer)
        {
            var identifiers = new NativeList<ObjectIdentifier>(RecordedObjects.Count, Allocator.Persistent);
            var localPositions = new NativeList<UnityEngine.Vector3>(RecordedObjects.Count, Allocator.Persistent);

            for (var idx = 0; idx < RecordedObjects.Count; ++idx)
            {
                var recordedObject = RecordedObjects[idx];
                var t = recordedObject.TypedObject;
                identifiers.Add(recordedObject.ObjectIdentifier);
                // localPositions.Add(t.localPosition);
                localPositions.Add(UnityEngine.Vector3.one);
            }

            await UniTask.SwitchToThreadPool();
            
            TransformUpdatePosition transformUpdatePositionSample;

            lock (_transformUpdatePositionPool)
            {
                transformUpdatePositionSample = _transformUpdatePositionPool.Get();
            }

            for (var idx = 0; idx < RecordedObjects.Count; ++idx)
            {
                transformUpdatePositionSample.LocalPosition.X = localPositions[idx].x;
                transformUpdatePositionSample.LocalPosition.Y = localPositions[idx].y;
                transformUpdatePositionSample.LocalPosition.Z = localPositions[idx].z;
                transformUpdatePositionSample.SerializeSampleToBuffer(_transformUpdatePositionSampleTypeUrlIndex, buffer);
            }

            lock (_transformUpdatePositionPool)
            {
                _transformUpdatePositionPool.Release(transformUpdatePositionSample);
            }

            identifiers.Dispose();
            localPositions.Dispose();
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