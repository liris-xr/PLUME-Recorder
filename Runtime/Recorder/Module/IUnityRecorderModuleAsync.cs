using Cysharp.Threading.Tasks;

namespace PLUME.Recorder.Module
{
    public interface IUnityFrameRecorderModuleAsync : IRecorderModule
    {
        internal UniTask RecordFrameAsync(FrameDataBuffer buffer);
    }
}