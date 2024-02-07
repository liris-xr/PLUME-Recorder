using System;
using System.Collections.Generic;
using System.Linq;
using PLUME.Sample.Unity.XRITK;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
#if XRITK_ENABLED
using InputAction = UnityEngine.InputSystem.InputAction;
using InputActionType = UnityEngine.InputSystem.InputActionType;
#endif

namespace PLUME.Recorder.Module.Unity.XRITK
{
#if !XRITK_ENABLED
    public class InputActionsRecorderModule : MonoBehaviour {}
#else
    
    //TODO : Refactor using generic callback from Action Map : https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Actions.html
    public class InputActionsRecorderModule : MonoBehaviour
    {
        [SerializeField]
        private bool recordAllActions = true;

        [SerializeField]
        private List<InputActionProperty> inputActionProperties;

        private InputActionManager _inputActionManager;

        private void Awake()
        {
            if(recordAllActions)
            {
                _inputActionManager = FindObjectOfType<InputActionManager>();

                if (_inputActionManager != null)
                {
                    foreach (InputActionAsset iaa in _inputActionManager.actionAssets)
                    {
                        foreach (InputActionMap iam in iaa.actionMaps)
                        {
                            iam.actionTriggered += RecordAction;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("No InputActionManager in scene. Add one if you want input actions to be recorded.");
                }
            }
            else
            {
                foreach(var iap in inputActionProperties)
                {
                    iap.action.started += RecordAction;
                }
            }
            
        }

        private void RecordAction(InputAction.CallbackContext ctx)
        {
            if (!Recorder.Instance.IsRecording)
                return;

            var inputActionSample = new PLUME.Sample.Unity.XRITK.InputAction();
            inputActionSample.Name = ctx.action.actionMap.name + '/' + ctx.action.name;
            inputActionSample.BindingPaths.AddRange(ctx.action.bindings.Select(b => b.path));

            switch (ctx.action.type)
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

            if (ctx.action.type == InputActionType.Button)
            {
                var b = ctx.ReadValueAsButton();
                var f = ctx.ReadValue<float>();
                
                var buttonValue = new ButtonValue();
                buttonValue.Boolean = b;
                buttonValue.Float = f;

                if (ctx.control is ButtonControl btnControl)
                    buttonValue.Threshold = btnControl.pressPointOrDefault;
                else
                    buttonValue.Threshold = InputSystem.settings.defaultButtonPressPoint;
            }
            else
            {
                var value = ctx.ReadValueAsObject();
            
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

            Recorder.Instance.RecordSampleStamped(inputActionSample);
        }

        private void OnDestroy()
        {
            if(recordAllActions)
            {
                _inputActionManager = FindObjectOfType<InputActionManager>();
                
                if(_inputActionManager != null) {
                    foreach (InputActionAsset iaa in _inputActionManager.actionAssets)
                    {
                        foreach (InputActionMap iam in iaa.actionMaps)
                        {
                            iam.actionTriggered -= RecordAction;
                        }
                    }
                }
            }
            else
            {
                foreach(var iap in inputActionProperties)
                {
                    iap.action.started -= RecordAction;
                }
            }
        }
    }
#endif
}