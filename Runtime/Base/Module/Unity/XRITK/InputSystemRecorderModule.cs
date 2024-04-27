#if XRITK_ENABLED
using System;
using System.Linq;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Utils;
using PLUME.Sample.Unity.XRITK;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Scripting;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using InputAction = UnityEngine.InputSystem.InputAction;
using InputActionType = UnityEngine.InputSystem.InputActionType;
using Object = UnityEngine.Object;

// TODO: this can be decoupled from XRITK by removing the dependency to InputActionManager
namespace PLUME.Base.Module.Unity.XRITK
{
    [Preserve]
    public class InputSystemRecorderModule : RecorderModule
    {
        private RecorderContext _ctx;
        private InputActionMap[] _recordedInputActionMaps;
        
        protected override void OnStartRecording(RecorderContext ctx)
        {
            base.OnStartRecording(ctx);
            _ctx = ctx;

            var inputActionManagers =
                Object.FindObjectsByType<InputActionManager>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            _recordedInputActionMaps = inputActionManagers
                .SelectMany(iam => iam.actionAssets.SelectMany(asset => asset.actionMaps))
                .ToArray();

            foreach (var inputActionMap in _recordedInputActionMaps)
            {
                inputActionMap.actionTriggered += OnActionTriggered;
            }
        }
        
        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            
            foreach (var inputActionMap in _recordedInputActionMaps)
            {
                inputActionMap.actionTriggered -= OnActionTriggered;
            }
        }
        
        private void OnActionTriggered(InputAction.CallbackContext context)
        {
            if (!_ctx.IsRecording)
                return;

            var inputActionSample = new Sample.Unity.XRITK.InputAction();
            inputActionSample.Name = context.action.actionMap.name + '/' + context.action.name;
            inputActionSample.BindingPaths.AddRange(context.action.bindings.Select(b => b.path));

            switch (context.action.type)
            {
                case InputActionType.Value:
                    inputActionSample.Type = Sample.Unity.XRITK.InputActionType.Value;
                    break;
                case InputActionType.Button:
                    inputActionSample.Type = Sample.Unity.XRITK.InputActionType.Button;
                    break;
                case InputActionType.PassThrough:
                    inputActionSample.Type = Sample.Unity.XRITK.InputActionType.Passthrough;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (context.action.type == InputActionType.Button)
            {
                var b = context.ReadValueAsButton();
                var f = context.ReadValue<float>();
                
                var buttonValue = new ButtonValue();
                buttonValue.Boolean = b;
                buttonValue.Float = f;

                if (context.control is ButtonControl btnControl)
                    buttonValue.Threshold = btnControl.pressPointOrDefault;
                else
                    buttonValue.Threshold = InputSystem.settings.defaultButtonPressPoint;
            }
            else
            {
                var value = context.ReadValueAsObject();
            
                switch (value)
                {
                    case bool b:
                        inputActionSample.Boolean = b;
                        break;
                    case int i:
                        inputActionSample.Integer = i;
                        break;
                    case float f:
                        inputActionSample.Float = f;
                        break;
                    case double d:
                        inputActionSample.Double = d;
                        break;
                    case Vector2 vec2:
                        inputActionSample.Vector2 = vec2.ToPayload();
                        break;
                    case Vector3 vec3:
                        inputActionSample.Vector3 = vec3.ToPayload();
                        break;
                    case Quaternion q:
                        inputActionSample.Quaternion = q.ToPayload();
                        break;
                }
            }

            _ctx.CurrentRecord.RecordTimestampedManagedSample(inputActionSample);
        }
    }
}
#endif