using PLUME.Base.Hooks;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Module.Unity.GameObject
{
    [Preserve]
    public class GameObjectRecorderModule : ObjectRecorderModule<UnityEngine.GameObject, GameObjectIdentifier,
        GameObjectSafeRef, GameObjectFrameData>
    {
        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            GameObjectHooks.OnCreate += go => OnCreate(go, ctx);
            ObjectHooks.OnBeforeDestroy += obj => OnBeforeDestroy(obj, ctx);
        }
        
        private void OnCreate(UnityEngine.GameObject go, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateGameObjectSafeRef(go);

            if (IsRecordingObject(objSafeRef))
                return;

            StartRecordingObject(objSafeRef, true, ctx);
        }
        
        private void OnBeforeDestroy(Object obj, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            if (obj is not UnityEngine.GameObject go)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateGameObjectSafeRef(go);

            if (!IsRecordingObject(objSafeRef))
                return;

            StopRecordingObject(objSafeRef, true, ctx);
        }
        
        protected override GameObjectFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            return new GameObjectFrameData();
        }
    }
}