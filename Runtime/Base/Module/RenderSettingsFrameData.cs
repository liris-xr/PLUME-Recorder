using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.Settings;

namespace PLUME.Base.Module
{
    public class RenderSettingsFrameData : PooledFrameData<RenderSettingsFrameData>
    {
        public static readonly FrameDataPool<RenderSettingsFrameData> Pool = new();

        private RenderSettingsUpdate _renderSettingsUpdateSample;

        public void SetRenderSettingsUpdateSample(RenderSettingsUpdate renderSettingsUpdateSample)
        {
            _renderSettingsUpdateSample = renderSettingsUpdateSample;
        }

        public override void Serialize(FrameDataWriter frameDataWriter)
        {
            if (_renderSettingsUpdateSample != null)
                frameDataWriter.WriteManaged(_renderSettingsUpdateSample);
        }

        public override void Clear()
        {
            _renderSettingsUpdateSample = null;
        }
    }
}