using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder.Module
{
    public interface IRecorderModule
    {
        internal void Create(RecorderContext context);

        internal void Destroy(RecorderContext context);

        internal void Start(RecordContext recordContext, RecorderContext recorderContext);

        internal UniTask Stop(RecordContext recordContext, RecorderContext recorderContext);

        internal void Reset(RecorderContext context);
    }
}