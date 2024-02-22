using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
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

        private NativeList<TransformUpdateLocalPositionSample> _currentFrameDirtySamples;
        private JobHandle _currentFramePollingJobHandle;

        protected override void OnCreate(RecorderContext ctx)
        {
            _transformAccessArray = new DynamicTransformAccessArray();
            _lastStates = new NativeHashMap<ObjectIdentifier, TransformState>(1000, Allocator.Persistent);
        }

        protected override void OnDestroy(RecorderContext ctx)
        {
            _transformAccessArray.Dispose();
            _lastStates.Dispose();

            _currentFramePollingJobHandle.Complete();

            if (_currentFrameDirtySamples.IsCreated)
                _currentFrameDirtySamples.Dispose();
        }
        
        private long lastUpdateTime;

        protected override void OnUpdate(long deltaTime, Record record, RecorderContext context)
        {
            var time = record.Time;
            Debug.Log((time - lastUpdateTime) / 1_000_000f + "ms");
            lastUpdateTime = time;
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

        protected override void OnStopRecording(Record record, RecorderContext recorderContext)
        {
            _currentFramePollingJobHandle.Complete();
            _transformAccessArray.Clear();
            _lastStates.Clear();
        }

        protected override void OnPreUpdate(long deltaTime, Record record, RecorderContext context)
        {
            _currentFrameDirtySamples =
                new NativeList<TransformUpdateLocalPositionSample>(RecordedObjects.Count, Allocator.Persistent);

            var pollTransformStatesJob = new PollTransformStatesJob
            {
                AlignedIdentifiers = _transformAccessArray.GetAlignedIdentifiers(),
                DirtySamples = _currentFrameDirtySamples.AsParallelWriter(),
                LastStates = _lastStates // TODO: work on a copy of _lastStates, update after job
            };

            _currentFramePollingJobHandle = pollTransformStatesJob.ScheduleReadOnly(_transformAccessArray, 128);
        }

        protected override TransformFrameData CollectFrameData(FrameInfo frameInfo)
        {
            _currentFramePollingJobHandle.Complete();
            var data = new TransformFrameData(_currentFrameDirtySamples);
            _currentFramePollingJobHandle = default;
            _currentFrameDirtySamples = default;
            return data;
        }
    }
}