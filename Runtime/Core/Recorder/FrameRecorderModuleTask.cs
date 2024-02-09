using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder
{
    public readonly struct FrameRecorderModuleTask
    {
        public readonly UniTask Task;
        public readonly FrameDataBuffer Buffer;

        public FrameRecorderModuleTask(UniTask task, FrameDataBuffer buffer)
        {
            Task = task;
            Buffer = buffer;
        }
    }
}