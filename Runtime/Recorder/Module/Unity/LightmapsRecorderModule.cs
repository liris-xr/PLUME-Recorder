using PLUME.Sample.Unity;
using UnityEngine;

namespace PLUME.Recorder.Module.Unity
{
    public class LightmapsRecorderModule : RecorderModule, IStartRecordingEventReceiver
    {
        public new void OnStartRecording()
        {
            base.OnStartRecording();

            var lightmapsUpdate = new LightmapsUpdate
            {
                LightmapsMode = LightmapSettings.lightmapsMode.ToPayload()
            };

            foreach (var lightmapData in LightmapSettings.lightmaps)
            {
                lightmapsUpdate.LightmapsData.Add(lightmapData.ToPayload());
            }

            recorder.RecordSampleStamped(lightmapsUpdate);
        }

        protected override void ResetCache()
        {
        }
    }
}