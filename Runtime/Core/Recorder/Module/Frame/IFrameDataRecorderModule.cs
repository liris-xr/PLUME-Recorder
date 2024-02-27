namespace PLUME.Core.Recorder.Module.Frame
{
    public interface IFrameDataRecorderModule : IRecorderModule
    {
        /// <summary>
        /// Enqueues frame data for the given frame info. This method is called by the frame recorder module after
        /// all modules LateUpdate.
        /// </summary>
        /// <param name="frameInfo"></param>
        /// <param name="record"></param>
        /// <param name="context"></param>
        internal void EnqueueFrameData(FrameInfo frameInfo, Record record, RecorderContext context);

        internal void PostEnqueueFrameData(Record record, RecorderContext context);
        
        internal void SerializeFrameData(FrameInfo frameInfo, FrameDataWriter frameDataWriter);

        internal void FixedUpdate(long fixedDeltaTime, Record record, RecorderContext context)
        {
        }

        internal void EarlyUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }

        internal void PreUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }

        internal void Update(long deltaTime, Record record, RecorderContext context)
        {
        }

        internal void PreLateUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }

        internal void PostLateUpdate(long deltaTime, Record record, RecorderContext context)
        {
        }
    }
}