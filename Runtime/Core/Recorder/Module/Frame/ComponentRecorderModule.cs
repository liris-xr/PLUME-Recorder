using System.Collections.Generic;
using PLUME.Base.Events;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using UnityEngine;
using IdentifierHashSet = Unity.Collections.NativeHashSet<PLUME.Core.Object.ComponentIdentifier>;

namespace PLUME.Core.Recorder.Module.Frame
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
            ObjectEvents.OnBeforeDestroyed += (obj, _) => OnBeforeDestroyed(obj, ctx);
            GameObjectEvents.OnComponentAdded += (go, component) => OnComponentAdded(go, component, ctx);
        }

        private void OnComponentAdded(UnityEngine.GameObject go, Component component, RecorderContext ctx)
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