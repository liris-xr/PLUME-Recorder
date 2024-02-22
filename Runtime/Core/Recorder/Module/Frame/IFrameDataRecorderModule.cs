namespace PLUME.Core.Recorder.Module.Frame
{
    public interface IFrameDataRecorderModule : IRecorderModule
    {
        internal void EnqueueFrameData(FrameInfo frameInfo);

        internal bool SerializeFrameData(FrameInfo frameInfo, FrameDataWriter frameDataWriter);

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