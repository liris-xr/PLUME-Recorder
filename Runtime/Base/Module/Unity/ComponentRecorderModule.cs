using System.Collections.Generic;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder.Module.Frame;
using UnityEngine;
using IdentifierHashSet = Unity.Collections.NativeHashSet<PLUME.Core.Object.ComponentIdentifier>;

namespace PLUME.Base.Module.Unity
{
    public abstract class
        ComponentRecorderModule<TC, TD> : ObjectRecorderModule<TC, ComponentIdentifier, ComponentSafeRef<TC>, TD>
        where TC : Component where TD : IFrameData
    {
        protected IReadOnlyList<ComponentSafeRef<TC>> RecordedComponents => RecordedObjects;

        protected IdentifierHashSet.ReadOnly RecordedComponentsIdentifier => RecordedObjectsIdentifier;

        protected IdentifierHashSet.ReadOnly CreatedComponentsIdentifier => CreatedObjectsIdentifier;

        protected IdentifierHashSet.ReadOnly DestroyedComponentsIdentifier => DestroyedObjectsIdentifier;
    }
}