using PLUME.Sample.Unity;
using UnityEngine.Rendering;

namespace PLUME
{
    public class RenderPipelineRecorderModule : RecorderModule, IStartRecordingEventReceiver
    {
        public new void OnStartRecording()
        {
            base.OnStartRecording();

            var renderPipelineAsset = GraphicsSettings.currentRenderPipeline;
            
            var renderPipelineName = renderPipelineAsset == null
                ? "Built-in Render Pipeline"
                : renderPipelineAsset.name;

            var renderPipelineUpdate = new RenderPipelineUpdate
            {
                Name = renderPipelineName,
                AssetId = renderPipelineAsset == null ? null : renderPipelineAsset.ToAssetIdentifierPayload()
            };

            recorder.RecordSample(renderPipelineUpdate);
        }

        protected override void ResetCache()
        {
        }
    }
}