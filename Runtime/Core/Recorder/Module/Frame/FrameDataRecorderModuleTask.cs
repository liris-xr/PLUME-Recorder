using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module.Frame
{
    public readonly struct FrameDataRecorderModuleTask
    {
        public readonly UniTask Task;
        public readonly SerializedSamplesBuffer Buffer;

        public FrameDataRecorderModuleTask(UniTask task, SerializedSamplesBuffer buffer)
        {
            Task = task;
            Buffer = buffer;
        }
    }
}