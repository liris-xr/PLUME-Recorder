using PLUME.Core.Recorder.Module;
using PLUME.Sample.Common;
using UnityEngine.Scripting;

namespace PLUME.Base.Module
{
    [Preserve]
    internal class MarkerRecorderModule : RecorderModule
    {
        private readonly Marker _marker = new();
        
        // TODO: add output parameter
        public void RecordMarker(string label)
        {
            EnsureIsRecording();
            _marker.Label = label;
            // _marker.SerializeSampleToBuffer();
        }
    }
}