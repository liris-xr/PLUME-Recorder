#if USE_XRITK_2 || USE_XRITK_3
using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.XRITK;
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
            interactable.activated.AddListener(args => OnActivated(objSafeRef, args));
            interactable.deactivated.AddListener(args => OnDeactivated(objSafeRef, args));
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
            interactable.activated.RemoveListener(args => OnActivated(objSafeRef, args));
            interactable.deactivated.RemoveListener(args => OnDeactivated(objSafeRef, args));
        }

        protected override void OnObjectMarkedCreated(XRBaseInteractableSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var interactable = objSafeRef.Component;
            var interactableUpdate = GetOrCreateUpdateSample(objSafeRef);
            interactableUpdate.Enabled = interactable.enabled;
            _createSamples[objSafeRef] = new XRBaseInteractableCreate
                { Id = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(XRBaseInteractableSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new XRBaseInteractableDestroy
                { Id = GetComponentIdentifierPayload(objSafeRef) };
        }

        private void OnHoverEntered(XRBaseInteractableSafeRef objSafeRef, HoverEnterEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactableHoverEnter = new XRBaseInteractableHoverEnter
            {
                Id = GetComponentIdentifierPayload(objSafeRef),
                InteractorCurrent = GetComponentIdentifierPayload(args.interactorObject.transform),
            };

            _ctx.CurrentRecord.RecordTimestampedManagedSample(interactableHoverEnter);
        }

        private void OnHoverExited(XRBaseInteractableSafeRef objSafeRef, HoverExitEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactableHoverExit = new XRBaseInteractableHoverExit
            {
                Id = GetComponentIdentifierPayload(objSafeRef),
                InteractorCurrent = GetComponentIdentifierPayload(args.interactorObject.transform),
            };

            _ctx.CurrentRecord.RecordTimestampedManagedSample(interactableHoverExit);
        }

        private void OnSelectEntered(XRBaseInteractableSafeRef objSafeRef, SelectEnterEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactableSelectEnter = new XRBaseInteractableSelectEnter
            {
                Id = GetComponentIdentifierPayload(objSafeRef),
                InteractorCurrent = GetComponentIdentifierPayload(args.interactorObject.transform),
            };

            _ctx.CurrentRecord.RecordTimestampedManagedSample(interactableSelectEnter);
        }

        private void OnSelectExited(XRBaseInteractableSafeRef objSafeRef, SelectExitEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactableSelectExit = new XRBaseInteractableSelectExit
            {
                Id = GetComponentIdentifierPayload(objSafeRef),
                InteractorCurrent = GetComponentIdentifierPayload(args.interactorObject.transform),
            };

            _ctx.CurrentRecord.RecordTimestampedManagedSample(interactableSelectExit);
        }

        private void OnActivated(XRBaseInteractableSafeRef objSafeRef, ActivateEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactableActivated = new XRBaseInteractableActivateEnter
            {
                Id = GetComponentIdentifierPayload(objSafeRef),
                InteractorCurrent = GetComponentIdentifierPayload(args.interactorObject.transform),
            };

            _ctx.CurrentRecord.RecordTimestampedManagedSample(interactableActivated);
        }

        private void OnDeactivated(XRBaseInteractableSafeRef objSafeRef, DeactivateEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactableDeactivated = new XRBaseInteractableActivateExit
            {
                Id = GetComponentIdentifierPayload(objSafeRef),
                InteractorCurrent = GetComponentIdentifierPayload(args.interactorObject.transform),
            };

            _ctx.CurrentRecord.RecordTimestampedManagedSample(interactableDeactivated);
        }

        private XRBaseInteractableUpdate GetOrCreateUpdateSample(XRBaseInteractableSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new XRBaseInteractableUpdate { Id = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override XRBaseInteractableFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = XRBaseInteractableFrameData.Pool.Get();
            frameData.AddCreateSamples(_createSamples.Values);
            frameData.AddDestroySamples(_destroySamples.Values);
            frameData.AddUpdateSamples(_updateSamples.Values);
            return frameData;
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
        }
    }
}
#endif