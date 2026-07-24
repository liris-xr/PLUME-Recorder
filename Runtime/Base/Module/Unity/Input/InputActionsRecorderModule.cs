#if INPUT_SYSTEM_ENABLED
using System;
using System.Collections.Generic;
using System.Linq;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Utils;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Scripting;
using InputAction = UnityEngine.InputSystem.InputAction;
using InputActionType = UnityEngine.InputSystem.InputActionType;
using Logger = PLUME.Core.Logger;

namespace PLUME.Base.Module.Unity.Input
{
    [Preserve]
    public class InputActionsRecorderModule : RecorderModule
    {
        private RecorderContext _ctx;
        private List<InputAction> _enabledActions;

        protected override void OnStartRecording(RecorderContext ctx)
        {
            base.OnStartRecording(ctx);
            _ctx = ctx;

            _enabledActions = InputSystem.ListEnabledActions();

            foreach (var action in _enabledActions)
            {
                action.performed += OnActionPerformed;
            }

            if (_enabledActions.Count == 0)
            {
                Logger.LogWarning("No enabled input action found. No input action will be recorded.");
            }
            else
            {
                var actionMapNames = _enabledActions
                    .Where(a => a.actionMap != null)
                    .Select(a => a.actionMap.asset == null
                        ? a.actionMap.name
                        : a.actionMap.asset.name + '/' + a.actionMap.name)
                    .Distinct()
                    .OrderBy(name => name)
                    .ToList();

                if (actionMapNames.Count == 0)
                    Logger.Log("Recording input actions from no action map (standalone actions only).");
                else
                    Logger.Log(
                        $"Recording {actionMapNames.Count} input action map(s): {string.Join(", ", actionMapNames)}");

                var actionNames = _enabledActions
                    .Select(a => (a.actionMap == null ? "" : a.actionMap.name + '/') + a.name)
                    .OrderBy(name => name);
                Logger.Log($"Recording {_enabledActions.Count} input action(s): {string.Join(", ", actionNames)}");
            }
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            
            foreach (var action in _enabledActions)
            {
                action.performed -= OnActionPerformed;
            }
        }
        
        private void OnActionPerformed(InputAction.CallbackContext context)
        {
            if (!_ctx.IsRecording)
                return;

            var inputActionSample = new Sample.Unity.InputAction
            {
                Name = context.action.actionMap.name + '/' + context.action.name
            };
            inputActionSample.BindingPaths.AddRange(context.action.bindings.Select(b => b.path));

            // TODO: add support for composite actions
            try
            {
                switch (context.action.type)
                {
                    case InputActionType.Value:
                        inputActionSample.Type = Sample.Unity.InputActionType.Value;
                        break;
                    case InputActionType.Button:
                        inputActionSample.Type = Sample.Unity.InputActionType.Button;
                        break;
                    case InputActionType.PassThrough:
                        inputActionSample.Type = Sample.Unity.InputActionType.Passthrough;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (context.action.type == InputActionType.Button)
                {
                    var b = context.ReadValueAsButton();
                    var f = context.ReadValue<float>();

                    var buttonValue = new ButtonValue
                    {
                        Boolean = b,
                        Float = f
                    };

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
            } catch (Exception e)
            {
                Debug.LogWarning($"Failed to record input action: {e}");
            }
        }
    }
}
#endif
