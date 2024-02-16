using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module
{
    public interface IRecorderModule
    {
        internal void Create(RecorderContext context);

        internal void Destroy(RecorderContext context);

        internal void StartRecording(RecordContext recordContext, RecorderContext recorderContext);

        internal UniTask StopRecording(RecordContext recordContext, RecorderContext recorderContext);

        internal void ForceStopRecording(RecordContext recordContext, RecorderContext recorderContext);

        internal void Reset(RecorderContext context);
        
        internal void FixedUpdate(RecordContext recordContext, RecorderContext context) {}
        
        internal void EarlyUpdate(RecordContext recordContext, RecorderContext context) {}
        
        internal void PreUpdate(RecordContext recordContext, RecorderContext context) {}
        
        internal void Update(RecordContext recordContext, RecorderContext context) {}
        
        internal void PreLateUpdate(RecordContext recordContext, RecorderContext context) {}
        
        internal void PostLateUpdate(RecordContext recordContext, RecorderContext context) {}
    }
}