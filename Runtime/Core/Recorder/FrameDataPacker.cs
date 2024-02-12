using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder
{
    public class FrameDataPacker
    {
        public async UniTask PackFrameData(long timestamp, int frame, SerializedSamplesBuffer buffer)
        {
            await UniTask.SwitchToThreadPool();
        }
    }
}