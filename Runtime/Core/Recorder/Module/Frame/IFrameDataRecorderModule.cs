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

        internal void FixedUpdate(ulong fixedDeltaTime, RecorderContext ctx)
        {
        }

        internal void EarlyUpdate(ulong deltaTime, RecorderContext ctx)
        {
        }

        internal void PreUpdate(ulong deltaTime, RecorderContext ctx)
        {
        }

        internal void Update(ulong deltaTime, RecorderContext ctx)
        {
        }

        internal void PreLateUpdate(ulong deltaTime, RecorderContext ctx)
        {
        }

        internal void PostLateUpdate(ulong deltaTime, RecorderContext ctx)
        {
        }
    }
}