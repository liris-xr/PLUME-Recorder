using PLUME.Base.Module;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using UnityObject = UnityEngine.Object;

namespace PLUME.Base
{
    public static class PlumeRecorderExtensions
    {
        public static void RecordMarker(this Recorder recorder, string label)
        {
            if (recorder.Context.TryGetRecorderModule<MarkerRecorderModuleBase>(out var module))
                module.RecordMarker(label);
        }

        public static void StartRecordingObject<T>(this Recorder recorder, ObjectSafeRef<T> objectSafeRef,
            bool markCreated) where T : UnityObject
        {
            recorder.EnsureIsRecording();

            foreach (var module in recorder.Context.Modules)
            {
                if (module is not IObjectRecorderModule objectRecorderModule)
                    continue;

                if (objectRecorderModule.SupportedObjectType != objectSafeRef.ObjectType)
                    continue;

                if (objectRecorderModule.IsRecordingObject(objectSafeRef))
                    continue;

                objectRecorderModule.StartRecordingObject(objectSafeRef, markCreated);
            }
        }

        public static void StopRecordingObject<T>(this Recorder recorder, ObjectSafeRef<T> objectSafeRef)
            where T : UnityObject
        {
            recorder.EnsureIsRecording();
            
            foreach (var module in recorder.Context.Modules)
            {
                if (module is not IObjectRecorderModule objectRecorderModule)
                    continue;
                
                if (objectRecorderModule.SupportedObjectType != objectSafeRef.ObjectType)
                    continue;
                
                if (!objectRecorderModule.IsRecordingObject(objectSafeRef))
                    continue;
                
                objectRecorderModule.StopRecordingObject(objectSafeRef);
            }
        }
    }
}