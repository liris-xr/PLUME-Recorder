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
    }
}