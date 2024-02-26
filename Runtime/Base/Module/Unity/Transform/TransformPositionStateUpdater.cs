using System;
using PLUME.Base.Module.Unity.Transform.Job;
using PLUME.Base.Module.Unity.Transform.State;
using PLUME.Core.Object;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace PLUME.Base.Module.Unity.Transform
{
    public class TransformPositionStateUpdater : IDisposable
    {
        private readonly float _angularThreshold;
        private readonly float _positionThresholdSq;
        private readonly float _scaleThresholdSq;
        
        private readonly DynamicTransformAccessArray _transformAccessArray;
        private NativeHashMap<ObjectIdentifier, PositionState> _positionStates;

        private NativeArray<ObjectIdentifier> _identifiersWorkCopy;
        private NativeArray<PositionState> _positionStatesWorkCopy;
        private JobHandle _pollNewPositionStatesJobHandle;

        public TransformPositionStateUpdater(NativeHashMap<ObjectIdentifier, PositionState> positionStates,
            DynamicTransformAccessArray transformAccessArray,
            float angularThreshold, float positionThresholdSq, float scaleThresholdSq)
        {
            _positionStates = positionStates;
            _transformAccessArray = transformAccessArray;
            
            _angularThreshold = angularThreshold;
            _positionThresholdSq = positionThresholdSq;
            _scaleThresholdSq = scaleThresholdSq;
        }

        public void StartPollingPositions()
        {
            _identifiersWorkCopy = _positionStates.GetKeyArray(Allocator.Persistent);
            _positionStatesWorkCopy = _positionStates.GetValueArray(Allocator.Persistent);

            var pollTransformStatesJob = new PollPositionStatesJob
            {
                PositionStates = _positionStatesWorkCopy,
                AngularThreshold = _angularThreshold,
                PositionThresholdSq = _positionThresholdSq,
                ScaleThresholdSq = _scaleThresholdSq,
            };

            _pollNewPositionStatesJobHandle = pollTransformStatesJob.ScheduleReadOnly(_transformAccessArray, 128);
        }

        public void Complete()
        {
            _pollNewPositionStatesJobHandle.Complete();
        }
        
        public void MergePolledPositions()
        {
            _pollNewPositionStatesJobHandle.Complete();

            new MergePositionStatesJob
            {
                PositionStates = _positionStates,
                PositionStatesWorkCopy = _positionStatesWorkCopy,
                IdentifiersWorkCopy = _identifiersWorkCopy
            }.RunBatch(_positionStatesWorkCopy.Length);
            
            _identifiersWorkCopy.Dispose();
            _positionStatesWorkCopy.Dispose();
            _identifiersWorkCopy = default;
            _positionStatesWorkCopy = default;
        }

        public void Dispose()
        {
            _pollNewPositionStatesJobHandle.Complete();
            _identifiersWorkCopy.Dispose();
            _positionStatesWorkCopy.Dispose();
        }
    }
}