using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace PLUME.Core.Recorder.ProtoBurst
{
    [BurstCompile]
    public struct SampleBatchComputeMaxSizeJob : IJob
    {
        // Input
        public NativeQueue<int> MaxSizes;

        // Output
        [WriteOnly]
        public NativeArray<int> MaxSize;

        public void Execute()
        {
            var maxSize = 0;
            while (MaxSizes.TryDequeue(out var size))
            {
                if (size > maxSize)
                    maxSize = size;
            }

            MaxSize[0] = maxSize;
        }
    }
}