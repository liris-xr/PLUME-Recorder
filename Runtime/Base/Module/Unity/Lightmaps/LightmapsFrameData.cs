using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;

namespace PLUME.Base.Module.Unity.Lightmaps
{
    public class LightmapsFrameData : PooledFrameData<LightmapsFrameData>
    {
        public static readonly FrameDataPool<LightmapsFrameData> Pool = new();

        private LightmapsUpdate _lightmapsUpdateSample;

        public void SetLightmapsUpdateSample(LightmapsUpdate lightmapsUpdateSample)
        {
            _lightmapsUpdateSample = lightmapsUpdateSample;
        }

        public override void Serialize(FrameDataWriter frameDataWriter)
        {
            if (_lightmapsUpdateSample != null)
                frameDataWriter.WriteManaged(_lightmapsUpdateSample);
        }

        public override void Clear()
        {
            _lightmapsUpdateSample = null;
        }
    }
}