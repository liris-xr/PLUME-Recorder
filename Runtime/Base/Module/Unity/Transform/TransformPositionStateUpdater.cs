using PLUME.Base.Module.Unity.Transform.Job;
using PLUME.Base.Module.Unity.Transform.State;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace PLUME.Base.Module.Unity.Transform
{
    internal class TransformPositionStateUpdater
    {
        private readonly float _angularThreshold;
        private readonly float _positionThresholdSq;
        private readonly float _scaleThresholdSq;

        private JobHandle _pollNewPositionStatesJobHandle;

        internal TransformPositionStateUpdater(
            float angularThreshold, float positionThresholdSq, float scaleThresholdSq)
        {
            _angularThreshold = angularThreshold;
            _positionThresholdSq = positionThresholdSq;
            _scaleThresholdSq = scaleThresholdSq;
        }

        public void UpdatePositionStates(
            NativeList<TransformPositionState> alignedPositionStates,
            DynamicTransformAccessArray transformAccessArray)
        {
            var pollTransformStatesJob = new UpdatePositionStatesJob
            {
                AlignedPositionStates = alignedPositionStates.AsArray(),
                AngularThreshold = _angularThreshold,
                PositionThresholdSq = _positionThresholdSq,
                ScaleThresholdSq = _scaleThresholdSq,
            };

            _pollNewPositionStatesJobHandle = pollTransformStatesJob.ScheduleReadOnly(transformAccessArray, 128);
            _pollNewPositionStatesJobHandle.Complete();
        }
    }
}