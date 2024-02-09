using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder
{
    public readonly struct FrameRecorderTask
    {
        public readonly int Frame;
        public readonly UniTask Task;

        public FrameRecorderTask(int frame, UniTask task)
        {
            Frame = frame;
            Task = task;
        }
    }
}