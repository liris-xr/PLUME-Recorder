using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;

namespace PLUME.Base.Module.Unity.GameObject
{
    public class
        GameObjectRecorderModule : ObjectFrameDataRecorderModuleBase<UnityEngine.GameObject, GameObjectFrameData>
    {
        protected override GameObjectFrameData CollectFrameData(FrameInfo frameInfo, Record record,
            RecorderContext context)
        {
            return new GameObjectFrameData();
        }
    }
}