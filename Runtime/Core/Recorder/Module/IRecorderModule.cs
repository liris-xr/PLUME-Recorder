using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module
{
    public interface IRecorderModule
    {
        protected bool IsRecording();
        
        internal void Create(RecorderContext context);

        internal void Destroy(RecorderContext context);

        internal void StartRecording(Record record, RecorderContext recorderContext);

        internal UniTask StopRecording(Record record, RecorderContext recorderContext);

        internal void ForceStopRecording(Record record, RecorderContext recorderContext);
        
        internal void EarlyUpdate(Record record, RecorderContext context) {}
        
        internal void PreUpdate(Record record, RecorderContext context) {}
        
        internal void Update(Record record, RecorderContext context) {}
        
        internal void PreLateUpdate(Record record, RecorderContext context) {}
        
        internal void PostLateUpdate(Record record, RecorderContext context) {}
    }
}