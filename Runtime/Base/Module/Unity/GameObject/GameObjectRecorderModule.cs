using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using UnityEngine.Scripting;

namespace PLUME.Base.Module.Unity.GameObject
{
    [Preserve]
    public class GameObjectRecorderModule : ObjectRecorderModule<UnityEngine.GameObject, GameObjectIdentifier,
        GameObjectSafeRef, GameObjectFrameData>
    {
        protected override GameObjectFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            return new GameObjectFrameData();
        }
    }
}