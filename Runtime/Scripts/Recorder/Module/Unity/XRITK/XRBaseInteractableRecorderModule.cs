using System.Collections.Generic;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.XRITK;
using UnityEngine;

#if XRITK_ENABLED
using UnityEngine.XR.Interaction.Toolkit;
#endif

namespace PLUME.XRITK
{
#if !XRITK_ENABLED
    public class XRBaseInteractableRecorderModule : MonoBehaviour {}
#else
    public class XRBaseInteractableRecorderModule : RecorderModule, IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, XRBaseInteractable> _recordedInteractables = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is XRBaseInteractable interactable &&
                !_recordedInteractables.ContainsKey(interactable.GetInstanceID()))
            {
                _recordedInteractables.Add(interactable.GetInstanceID(), interactable);
                RecordCreation(interactable);

                interactable.hoverEntered.AddListener(RecordHoverEntered);
                interactable.hoverExited.AddListener(RecordHoverExited);

                interactable.selectEntered.AddListener(RecordSelectEntered);
                interactable.selectExited.AddListener(RecordSelectExited);

                interactable.activated.AddListener(RecordActivated);
                interactable.deactivated.AddListener(RecordDeactivated);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedInteractables.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RemoveFromCache(int interactableInstanceId)
        {
            _recordedInteractables.Remove(interactableInstanceId);
        }

        private void RecordCreation(XRBaseInteractable interactable)
        {
            var interactableCreate = new XRBaseInteractableCreate {Id = interactable.ToIdentifierPayload()};

            var interactableSetEnabled = new XRBaseInteractableSetEnabled
            {
                Id = interactable.ToIdentifierPayload(),
                Enabled = interactable.enabled
            };

            recorder.RecordSampleStamped(interactableCreate);
            recorder.RecordSampleStamped(interactableSetEnabled);
        }

        private void RecordDestruction(int interactableInstanceId)
        {
            var interactableDestroyed = new ComponentDestroy
                {Id = new ComponentDestroyIdentifier {Id = interactableInstanceId.ToString()}};
            recorder.RecordSampleStamped(interactableDestroyed);
        }

        void RecordHoverEntered(HoverEnterEventArgs args)
        {
            var interactable = args.interactableObject.transform.GetComponent<XRBaseInteractable>();

            var interactableHoverEnter = new XRBaseInteractableHoverEnter()
            {
                Id = interactable.ToIdentifierPayload(),
                InteractorCurrent = args.interactorObject.transform.ToIdentifierPayload(),
            };

            recorder.RecordSampleStamped(interactableHoverEnter);
        }

        void RecordHoverExited(HoverExitEventArgs args)
        {
            var interactable = args.interactableObject.transform.GetComponent<XRBaseInteractable>();

            var interactableHoverExit = new XRBaseInteractableHoverExit()
            {
                Id = interactable.ToIdentifierPayload(),
                InteractorCurrent = args.interactorObject.transform.ToIdentifierPayload(),
            };

            recorder.RecordSampleStamped(interactableHoverExit);
        }

        void RecordSelectEntered(SelectEnterEventArgs args)
        {
            var interactable = args.interactableObject.transform.GetComponent<XRBaseInteractable>();

            var interactableSelectEnter = new XRBaseInteractableSelectEnter()
            {
                Id = interactable.ToIdentifierPayload(),
                InteractorCurrent = args.interactorObject.transform.ToIdentifierPayload(),
            };

            recorder.RecordSampleStamped(interactableSelectEnter);
        }

        void RecordSelectExited(SelectExitEventArgs args)
        {
            var interactable = args.interactableObject.transform.GetComponent<XRBaseInteractable>();

            var interactableSelectExit = new XRBaseInteractableSelectExit()
            {
                Id = interactable.ToIdentifierPayload(),
                InteractorCurrent = args.interactorObject.transform.ToIdentifierPayload(),
            };

            recorder.RecordSampleStamped(interactableSelectExit);
        }

        void RecordActivated(ActivateEventArgs args)
        {
            var interactable = args.interactableObject.transform.GetComponent<XRBaseInteractable>();

            var interactableActivateEnter = new XRBaseInteractableActivateEnter()
            {
                Id = interactable.ToIdentifierPayload(),
                InteractorCurrent = args.interactorObject.transform.ToIdentifierPayload(),
            };

            recorder.RecordSampleStamped(interactableActivateEnter);
        }

        void RecordDeactivated(DeactivateEventArgs args)
        {
            var interactable = args.interactableObject.transform.GetComponent<XRBaseInteractable>();

            var interactableActivateExit = new XRBaseInteractableActivateExit()
            {
                Id = interactable.ToIdentifierPayload(),
                InteractorCurrent = args.interactorObject.transform.ToIdentifierPayload(),
            };

            recorder.RecordSampleStamped(interactableActivateExit);
        }

        protected override void ResetCache()
        {
            _recordedInteractables.Clear();
        }
    }
#endif
}