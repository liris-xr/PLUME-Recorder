using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;

namespace PLUME.Base.Module.Unity.GameObject
{
    public class GameObjectRecorderModule :
        ObjectRecorderModule<UnityEngine.GameObject, GameObjectIdentifier, GameObjectSafeRef, GameObjectFrameData>
    {
        protected override GameObjectFrameData CollectFrameData(FrameInfo frameInfo, Record record, RecorderContext ctx)
        {
            return new GameObjectFrameData();
        }
    }
}