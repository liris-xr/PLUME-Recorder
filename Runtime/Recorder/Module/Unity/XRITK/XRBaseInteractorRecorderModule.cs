using System.Collections.Generic;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.XRITK;
using UnityEngine;
#if XRITK_ENABLED
using UnityEngine.XR.Interaction.Toolkit;
#endif

namespace PLUME
{
#if !XRITK_ENABLED
    public class XRBaseInteractorRecorderModule : MonoBehaviour
    {
    }
#else
    public class XRBaseInteractorRecorderModule : RecorderModule, IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, XRBaseInteractor> _recordedInteractors = new();

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is XRBaseInteractor interactor && !_recordedInteractors.ContainsKey(interactor.GetInstanceID()))
            {
                _recordedInteractors.Add(interactor.GetInstanceID(), interactor);
                RecordCreation(interactor);

                interactor.hoverEntered.AddListener(RecordHoverEntered);
                interactor.hoverExited.AddListener(RecordHoverExited);

                interactor.selectEntered.AddListener(RecordSelectEntered);
                interactor.selectExited.AddListener(RecordSelectExited);
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedInteractors.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RemoveFromCache(int interactorInstanceId)
        {
            _recordedInteractors.Remove(interactorInstanceId);
        }

        private void RecordCreation(XRBaseInteractor interactor)
        {
            var interactorCreate = new XRBaseInteractorCreate {Id = interactor.ToIdentifierPayload()};

            var interactorSetEnabled = new XRBaseInteractorSetEnabled
            {
                Id = interactor.ToIdentifierPayload(),
                Enabled = interactor.enabled
            };

            recorder.RecordSampleStamped(interactorCreate);
            recorder.RecordSampleStamped(interactorSetEnabled);
        }

        private void RecordDestruction(int interactorInstanceId)
        {
            var interactorDestroyed = new ComponentDestroy
                {Id = new ComponentDestroyIdentifier {Id = interactorInstanceId.ToString()}};
            recorder.RecordSampleStamped(interactorDestroyed);
        }

        void RecordHoverEntered(HoverEnterEventArgs args)
        {
            var interactor = args.interactorObject.transform.GetComponent<XRBaseInteractor>();

            var interactorHoverEnter = new XRBaseInteractorHoverEnter()
            {
                Id = interactor.ToIdentifierPayload(),
                InteractableCurrent = args.interactableObject.transform.ToIdentifierPayload()
            };

            recorder.RecordSampleStamped(interactorHoverEnter);
        }

        void RecordHoverExited(HoverExitEventArgs args)
        {
            var interactor = args.interactorObject.transform.GetComponent<XRBaseInteractor>();

            var interactorHoverExit = new XRBaseInteractorHoverExit()
            {
                Id = interactor.ToIdentifierPayload(),
                InteractableCurrent = args.interactableObject.transform.ToIdentifierPayload()
            };

            recorder.RecordSampleStamped(interactorHoverExit);
        }

        void RecordSelectEntered(SelectEnterEventArgs args)
        {
            var interactor = args.interactorObject.transform.GetComponent<XRBaseInteractor>();

            var interactorSelectEnter = new XRBaseInteractorHoverEnter()
            {
                Id = interactor.ToIdentifierPayload(),
                InteractableCurrent = args.interactableObject.transform.ToIdentifierPayload()
            };

            recorder.RecordSampleStamped(interactorSelectEnter);
        }

        void RecordSelectExited(SelectExitEventArgs args)
        {
            var interactor = args.interactorObject.transform.GetComponent<XRBaseInteractor>();

            var interactorSelectExit = new XRBaseInteractorSelectExit()
            {
                Id = interactor.ToIdentifierPayload(),
                InteractableCurrent = args.interactableObject.transform.ToIdentifierPayload()
            };

            recorder.RecordSampleStamped(interactorSelectExit);
        }

        protected override void ResetCache()
        {
            _recordedInteractors.Clear();
        }
    }
#endif
}