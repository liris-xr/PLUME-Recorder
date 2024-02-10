using System;
using PLUME.Core.Object.SafeRef;

namespace PLUME.Core.Recorder.Module
{
    public interface IObjectRecorderModule : IRecorderModule
    {
        public Type SupportedObjectType { get; }

        public void StartRecordingObject(IObjectSafeRef objSafeRef, bool markCreated = true);

        public void StopRecordingObject(IObjectSafeRef objSafeRef, bool markDestroyed = true);

        public bool IsRecordingObject(IObjectSafeRef objSafeRef);
    }
}