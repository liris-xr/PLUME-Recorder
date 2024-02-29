using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using UnityEngine.Scripting;

namespace PLUME.Base.Module.Unity.Camera
{
    [Preserve]
    public class CameraRecorderModule : ComponentRecorderModule<UnityEngine.Camera, CameraFrameData>
    {
        protected override CameraFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            throw new System.NotImplementedException();
        }
        
        //TODO : Implement recorder module
    }
}