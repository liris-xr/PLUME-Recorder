using System.Collections.Generic;
using PLUME.Base.Hooks;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using UnityEngine;
using IdentifierHashSet = Unity.Collections.NativeHashSet<PLUME.Core.Object.ComponentIdentifier>;

namespace PLUME.Base.Module.Unity
{
    public abstract class
        ComponentRecorderModule<TC, TD> : ObjectRecorderModule<TC, ComponentIdentifier, IComponentSafeRef<TC>, TD>
        where TC : Component where TD : IFrameData
    {
        protected IReadOnlyList<IComponentSafeRef<TC>> RecordedComponents => RecordedObjects;

        protected IdentifierHashSet.ReadOnly RecordedComponentsIdentifier => RecordedObjectsIdentifier;

        protected IdentifierHashSet.ReadOnly CreatedComponentsIdentifier => CreatedObjectsIdentifier;

        protected IdentifierHashSet.ReadOnly DestroyedComponentsIdentifier => DestroyedObjectsIdentifier;

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            ObjectHooks.OnBeforeDestroy += obj => OnBeforeDestroy(obj, ctx);
            GameObjectHooks.OnAddComponent += (go, component) => OnAddComponent(go, component, ctx);
        }

        private void OnAddComponent(UnityEngine.GameObject go, Component component, RecorderContext ctx)
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

        private void OnBeforeDestroy(Object obj, RecorderContext ctx)
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