using System.Collections.Generic;
using PLUME.Base.Hooks;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using UnityEngine;

namespace PLUME.Core.Recorder.Module.Frame
{
    public abstract class
        ComponentRecorderModule<TC, TD> : ObjectRecorderModule<ComponentIdentifier, IComponentSafeRef<TC>, TD>
        where TC : Component where TD : IFrameData
    {
        protected IReadOnlyList<IComponentSafeRef<TC>> RecordedComponents => RecordedObjects;

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            ObjectHooks.OnBeforeDestroyed += (obj, _) => OnBeforeDestroyed(obj, ctx);
            GameObjectHooks.OnComponentAdded += (go, component) => OnComponentAdded(go, component, ctx);
        }

        private void OnComponentAdded(GameObject go, Component component, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            if (component is not TC c)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(c);

            if (IsRecordingObject(objSafeRef))
                return;

            StartRecordingObject(objSafeRef, true, ctx);
        }

        private void OnBeforeDestroyed(UnityEngine.Object obj, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            if (obj is not TC c)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(c);

            if (!IsRecordingObject(objSafeRef))
                return;

            StopRecordingObject(objSafeRef, true, ctx);
        }
    }
}