using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module.Frame
{
    public readonly struct FrameRecorderModuleTask
    {
        public readonly int Frame;
        public readonly UniTask Task;

        public FrameRecorderModuleTask(int frame, UniTask task)
        {
            Frame = frame;
            Task = task;
        }
    }
}