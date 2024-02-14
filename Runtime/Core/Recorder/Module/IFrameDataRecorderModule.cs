using System.Threading;
using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module
{
    public interface IFrameDataRecorderModule : IRecorderModule
    {
        internal UniTask RecordFrameData(SerializedSamplesBuffer buffer, CancellationToken cancellationToken = default);
    }
}