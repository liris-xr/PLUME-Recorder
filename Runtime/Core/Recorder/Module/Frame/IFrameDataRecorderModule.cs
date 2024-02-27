namespace PLUME.Core.Recorder.Module.Frame
{
    public interface IFrameDataRecorderModule : IRecorderModule
    {
        /// <summary>
        /// Enqueues frame data for the given frame info. This method is called by the frame recorder module after
        /// all modules LateUpdate.
        /// </summary>
        /// <param name="frameInfo"></param>
        /// <param name="ctx"></param>
        internal void EnqueueFrameData(FrameInfo frameInfo, RecorderContext ctx);

        internal void PostEnqueueFrameData(RecorderContext ctx);
        
        internal void SerializeFrameData(FrameInfo frameInfo, FrameDataWriter frameDataWriter);

        internal void FixedUpdate(long fixedDeltaTime, RecorderContext ctx)
        {
        }

        internal void EarlyUpdate(long deltaTime, RecorderContext ctx)
        {
        }

        internal void PreUpdate(long deltaTime, RecorderContext ctx)
        {
        }

        internal void Update(long deltaTime, RecorderContext ctx)
        {
        }

        internal void PreLateUpdate(long deltaTime, RecorderContext ctx)
        {
        }

        internal void PostLateUpdate(long deltaTime, RecorderContext ctx)
        {
        }
    }
}