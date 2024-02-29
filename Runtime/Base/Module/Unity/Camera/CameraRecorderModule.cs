using PLUME.Base.Module.Unity;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using UnityEngine.Scripting;
using CameraSafeRef = PLUME.Core.Object.SafeRef.ComponentSafeRef<UnityEngine.Camera>;

namespace PLUME
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