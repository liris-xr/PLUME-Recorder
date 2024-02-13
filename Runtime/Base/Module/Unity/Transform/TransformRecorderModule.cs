using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using PLUME.Core;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Utils.Sample;
using PLUME.Sample.Unity;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Pool;
using UnityEngine.Scripting;
using Vector3 = PLUME.Sample.Common.Vector3;

namespace PLUME.Base.Module.Unity.Transform
{
    [Preserve]
    internal class TransformRecorderModule : ObjectFrameDataRecorderModuleAsync<UnityEngine.Transform>
    {
        private DynamicTransformAccessArray _transformAccessArray;
        private NativeHashMap<ObjectIdentifier, TransformState> _lastStates;
        private SampleTypeUrlIndex _updatePosSampleTypeUrlIndex;

        private readonly ObjectPool<TransformUpdateLocalPosition> _transformUpdatePositionPool = new(() =>
            new TransformUpdateLocalPosition
            {
                Id = new TransformGameObjectIdentifier(),
                LocalPosition = new Vector3()
            });
        
        protected override void OnCreate(ObjectSafeRefProvider objSafeRefProvider, SampleTypeUrlRegistry typeUrlRegistry)
        {
            _transformAccessArray = new DynamicTransformAccessArray();
            _lastStates = new NativeHashMap<ObjectIdentifier, TransformState>(1000, Allocator.Persistent);
            _updatePosSampleTypeUrlIndex = typeUrlRegistry.GetOrCreateTypeUrlIndex("fr.liris.plume", TransformUpdateLocalPosition.Descriptor);
        }

        protected override void OnDestroy()
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

        protected override void OnStartRecording(ObjectSafeRef<UnityEngine.Transform> objSafeRef, bool markCreated)
        {
            _transformAccessArray.TryAdd(objSafeRef);
            _lastStates[objSafeRef.Identifier] = TransformState.Null;
        }

        protected override void OnStopRecording(ObjectSafeRef<UnityEngine.Transform> objSafeRef, bool markDestroyed)
        {
            _transformAccessArray.RemoveSwapBack(objSafeRef);
            _lastStates.Remove(objSafeRef.Identifier);
        }

        protected override void OnReset()
        {
            _transformAccessArray.Clear();
            _lastStates.Clear();
        }

        [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        protected override async UniTask OnRecordFrameData(SerializedSamplesBuffer buffer)
        {
            var identifiers = new NativeList<ObjectIdentifier>(RecordedObjects.Count, Allocator.Persistent);
            var localPositions = new NativeList<UnityEngine.Vector3>(RecordedObjects.Count, Allocator.Persistent);
            
            for (var idx = 0; idx < RecordedObjects.Count; ++idx)
            {
                var recordedObject = RecordedObjects[idx];
                var t = recordedObject.TypedObject;
                identifiers.Add(recordedObject.Identifier);
                // localPositions.Add(t.localPosition);
                localPositions.Add(UnityEngine.Vector3.one);
            }
            
            // // TODO: run in a job
            await UniTask.SwitchToThreadPool();
            
            TransformUpdateLocalPosition transformUpdatePositionSample;
            
            lock (_transformUpdatePositionPool)
            {
                transformUpdatePositionSample = _transformUpdatePositionPool.Get();
            }
            
            for (var idx = 0; idx < RecordedObjects.Count; ++idx)
            {
                transformUpdatePositionSample.LocalPosition.X = localPositions[idx].x;
                transformUpdatePositionSample.LocalPosition.Y = localPositions[idx].y;
                transformUpdatePositionSample.LocalPosition.Z = localPositions[idx].z;
                transformUpdatePositionSample.SerializeSampleToBuffer(_updatePosSampleTypeUrlIndex, buffer);
            }
            
            lock (_transformUpdatePositionPool)
            {
                _transformUpdatePositionPool.Release(transformUpdatePositionSample);
            }
            
            await UniTask.SwitchToMainThread();

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