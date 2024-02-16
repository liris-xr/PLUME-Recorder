using PLUME.Core;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Sample.Common;
using UnityEngine.Scripting;

namespace PLUME.Base.Module
{
    [Preserve]
    internal class MarkerRecorderModuleBase : RecorderModuleBase
    {
        private readonly Marker _marker = new();
        private SampleTypeUrlIndex _markerSampleTypeUrlIndex;
        private RecordContext _recordContext;
        
        protected override void OnCreate(RecorderContext ctx)
        {
            _markerSampleTypeUrlIndex = ctx.SampleTypeUrlRegistry.GetOrCreateTypeUrlIndex("fr.liris.plume/plume.sample.common.Marker");
        }

        protected override void OnStartRecording(RecordContext recordContext, RecorderContext recorderContext)
        {
            _recordContext = recordContext;
        }

        // TODO: add output parameter
        public void RecordMarker(string label)
        {
            CheckIsRecording();
            _marker.Label = label;
            _recordContext.Data.PushTimestampedSample(_marker, _recordContext.Clock.ElapsedNanoseconds);
        }
    }
}