using System;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Rendering;

namespace PLUME
{
    public class QualitySettingsRecorderModule : RecorderModule, IStartRecordingEventReceiver
    {
        public new void OnStartRecording()
        {
            base.OnStartRecording();

            var qualityLevelName = "";

            try
            {
                qualityLevelName = QualitySettings.names[QualitySettings.GetQualityLevel()];
            }
            catch (Exception)
            {
                // ignored
            }

            var renderPipelineAsset = QualitySettings.renderPipeline;

            var renderPipelineUpdate = new QualitySettingsUpdate
            {
                Name = qualityLevelName,
                RenderPipelineAssetId = renderPipelineAsset == null ? null : renderPipelineAsset.ToAssetIdentifierPayload()
            };

            recorder.RecordSampleStamped(renderPipelineUpdate);
        }

        protected override void ResetCache()
        {
        }
    }
}