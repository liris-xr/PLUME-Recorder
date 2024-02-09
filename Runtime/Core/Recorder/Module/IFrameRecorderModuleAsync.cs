using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module
{
    public interface IFrameRecorderModuleAsync : IRecorderModule
    {
        internal UniTask RecordFrameDataAsync(FrameDataBuffer buffer);
    }
}