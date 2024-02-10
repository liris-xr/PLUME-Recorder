using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module.Frame
{
    public interface IFrameDataRecorderModuleAsync : IRecorderModule
    {
        internal UniTask RecordFrameDataAsync(SerializedSamplesBuffer buffer);
    }
}