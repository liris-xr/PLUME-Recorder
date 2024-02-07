using PLUME.Sample.Unity;
using UnityEngine.Rendering;

namespace PLUME.Recorder.Module.Unity
{
    public class GraphicsSettingsRecorderModule : RecorderModule, IStartRecordingEventReceiver
    {
        public new void OnStartRecording()
        {
            base.OnStartRecording();

            var renderPipelineAsset = GraphicsSettings.defaultRenderPipeline;

            var renderPipelineUpdate = new GraphicsSettingsUpdate
            {
                DefaultRenderPipelineAssetId =
                    renderPipelineAsset == null ? null : renderPipelineAsset.ToAssetIdentifierPayload()
            };

            recorder.RecordSampleStamped(renderPipelineUpdate);
        }

        protected override void ResetCache()
        {
        }
    }
}