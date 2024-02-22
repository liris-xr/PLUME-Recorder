namespace PLUME.Core.Recorder.Module
{
    public interface IRecorderModule
    {
        public bool IsRecording { get; }

        internal void Create(RecorderContext context);

        internal void Destroy(RecorderContext context);

        internal void Awake(RecorderContext context);

        internal void StartRecording(Record record, RecorderContext recorderContext);

        internal void StopRecording(Record record, RecorderContext recorderContext);

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