#if USE_XRITK_2 || USE_XRITK_3
using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.XRITK;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.Interaction.Toolkit;
using static PLUME.Core.Utils.SampleUtils;

#if USE_XRITK_2
using XRBaseInteractableSafeRef =
    PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.XR.Interaction.Toolkit.XRBaseInteractable>;
#elif USE_XRITK_3
using XRBaseInteractableSafeRef =
    PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>;
#endif

namespace PLUME.Base.Module.Unity.XRITK
{
    [Preserve]
#if USE_XRITK_2
    public class XRBaseInteractableRecorderModule : ComponentRecorderModule<UnityEngine.XR.Interaction.Toolkit.XRBaseInteractable, XRBaseInteractableFrameData>
#elif USE_XRITK_3
    public class XRBaseInteractableRecorderModule : ComponentRecorderModule<
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable, XRBaseInteractableFrameData>
#endif
    {
        private readonly Dictionary<XRBaseInteractableSafeRef, XRBaseInteractableCreate> _createSamples = new();
        private readonly Dictionary<XRBaseInteractableSafeRef, XRBaseInteractableDestroy> _destroySamples = new();
        private readonly Dictionary<XRBaseInteractableSafeRef, XRBaseInteractableUpdate> _updateSamples = new();
        private readonly List<XRITKInteraction> _interactionSamples = new();

        private RecorderContext _ctx;

        protected override void OnStartRecording(RecorderContext ctx)
        {
            base.OnStartRecording(ctx);
            _ctx = ctx;
        }

        protected override void OnStartRecordingObject(XRBaseInteractableSafeRef objSafeRef,
            RecorderContext ctx)
        {
            base.OnStartRecordingObject(objSafeRef, ctx);
            var interactable = objSafeRef.Component;
            interactable.hoverEntered.AddListener(args => OnHoverEntered(objSafeRef, args));
            interactable.hoverExited.AddListener(args => OnHoverExited(objSafeRef, args));
            interactable.selectEntered.AddListener(args => OnSelectEntered(objSafeRef, args));
            interactable.selectExited.AddListener(args => OnSelectExited(objSafeRef, args));
            interactable.activated.AddListener(args => OnActivateEnter(objSafeRef, args));
            interactable.deactivated.AddListener(args => OnActivateExit(objSafeRef, args));
        }

        protected override void OnStopRecordingObject(XRBaseInteractableSafeRef objSafeRef,
            RecorderContext ctx)
        {
            base.OnStopRecordingObject(objSafeRef, ctx);
            var interactable = objSafeRef.Component;
            interactable.hoverEntered.RemoveListener(args => OnHoverEntered(objSafeRef, args));
            interactable.hoverExited.RemoveListener(args => OnHoverExited(objSafeRef, args));
            interactable.selectEntered.RemoveListener(args => OnSelectEntered(objSafeRef, args));
            interactable.selectExited.RemoveListener(args => OnSelectExited(objSafeRef, args));
            interactable.activated.RemoveListener(args => OnActivateEnter(objSafeRef, args));
            interactable.deactivated.RemoveListener(args => OnActivateExit(objSafeRef, args));
        }

        protected override void OnObjectMarkedCreated(XRBaseInteractableSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var interactable = objSafeRef.Component;
            var interactableUpdate = GetOrCreateUpdateSample(objSafeRef);
            interactableUpdate.Enabled = interactable.enabled;
            _createSamples[objSafeRef] = new XRBaseInteractableCreate
                { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(XRBaseInteractableSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new XRBaseInteractableDestroy
                { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        private void OnHoverEntered(XRBaseInteractableSafeRef objSafeRef, HoverEnterEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;
            
            var interactor = (Component) args.interactorObject;
            
            var interactableHoverEnter = new XRITKInteraction
            {
                Interactable = GetComponentIdentifierPayload(objSafeRef),
                Interactor = GetComponentIdentifierPayload(interactor),
                Type = XRITKInteractionType.HoverEnter
            };

            _interactionSamples.Add(interactableHoverEnter);
        }

        private void OnHoverExited(XRBaseInteractableSafeRef objSafeRef, HoverExitEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactor = (Component) args.interactorObject;
            
            var interactableHoverExit = new XRITKInteraction
            {
                Interactable = GetComponentIdentifierPayload(objSafeRef),
                Interactor = GetComponentIdentifierPayload(interactor),
                Type = XRITKInteractionType.HoverExit
            };

            _interactionSamples.Add(interactableHoverExit);
        }

        private void OnSelectEntered(XRBaseInteractableSafeRef objSafeRef, SelectEnterEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactor = (Component) args.interactorObject;
            
            var interactableSelectEnter = new XRITKInteraction
            {
                Interactable = GetComponentIdentifierPayload(objSafeRef),
                Interactor = GetComponentIdentifierPayload(interactor),
                Type = XRITKInteractionType.SelectEnter
            };

            _interactionSamples.Add(interactableSelectEnter);
        }

        private void OnSelectExited(XRBaseInteractableSafeRef objSafeRef, SelectExitEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactor = (Component) args.interactorObject;
            
            var interactableSelectExit = new XRITKInteraction
            {
                Interactable = GetComponentIdentifierPayload(objSafeRef),
                Interactor = GetComponentIdentifierPayload(interactor),
                Type = XRITKInteractionType.SelectExit
            };

            _interactionSamples.Add(interactableSelectExit);
        }

        private void OnActivateEnter(XRBaseInteractableSafeRef objSafeRef, ActivateEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactor = (Component) args.interactorObject;
            
            var interactableActivateEnter = new XRITKInteraction
            {
                Interactable = GetComponentIdentifierPayload(objSafeRef),
                Interactor = GetComponentIdentifierPayload(interactor),
                Type = XRITKInteractionType.ActivateEnter
            };
            
            _interactionSamples.Add(interactableActivateEnter);
        }

        private void OnActivateExit(XRBaseInteractableSafeRef objSafeRef, DeactivateEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactor = (Component) args.interactorObject;
            
            var interactableActivateExit = new XRITKInteraction
            {
                Interactable = GetComponentIdentifierPayload(objSafeRef),
                Interactor = GetComponentIdentifierPayload(interactor),
                Type = XRITKInteractionType.ActivateExit
            };
            
            _interactionSamples.Add(interactableActivateExit);
        }

        private XRBaseInteractableUpdate GetOrCreateUpdateSample(XRBaseInteractableSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new XRBaseInteractableUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override XRBaseInteractableFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = XRBaseInteractableFrameData.Pool.Get();
            frameData.AddCreateSamples(_createSamples.Values);
            frameData.AddDestroySamples(_destroySamples.Values);
            frameData.AddUpdateSamples(_updateSamples.Values);
            frameData.AddInteractionSamples(_interactionSamples);
            return frameData;
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
            _interactionSamples.Clear();
        }
    }
}
#endif