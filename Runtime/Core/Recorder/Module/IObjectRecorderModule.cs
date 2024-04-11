using PLUME.Core.Object.SafeRef;

namespace PLUME.Core.Recorder.Module
{
    public interface IObjectRecorderModule : IRecorderModule
    {
        /**
         * Start recording an object if the object is not already being recorded and is compatible with the recorder module.
         *
         * @param objSafeRef The object to start recording.
         * @param markCreated Whether to mark the object as created, most of the time producing creation samples with initial update data.
         * @param ctx The recorder context.
         * @return Whether the object was successfully started recording.
         */
        public bool StartRecordingObject(IObjectSafeRef objSafeRef, bool markCreated, RecorderContext ctx);

        /**
         * Stop recording an object if the object is being recorded.
         *
         * @param objSafeRef The object to stop recording.
         * @param markDestroyed Whether to mark the object as destroyed, most of the time producing destruction samples.
         * @param ctx The recorder context.
         * @return Whether the object was successfully stopped recording.
         */
        public bool StopRecordingObject(IObjectSafeRef objSafeRef, bool markDestroyed, RecorderContext ctx);
    }
}