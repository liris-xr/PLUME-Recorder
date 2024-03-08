#if XRITK_ENABLED
using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.XRITK;
using UnityEngine.Scripting;
using UnityEngine.XR.Interaction.Toolkit;
using static PLUME.Core.Utils.SampleUtils;
using XRBaseInteractorSafeRef =
    PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.XR.Interaction.Toolkit.XRBaseInteractor>;

namespace PLUME.Base.Module.Unity.XRITK
{
    [Preserve]
    public class XRBaseInteractorRecorderModule : ComponentRecorderModule<XRBaseInteractor, XRBaseInteractorFrameData>
    {
        private readonly Dictionary<XRBaseInteractorSafeRef, XRBaseInteractorCreate> _createSamples = new();
        private readonly Dictionary<XRBaseInteractorSafeRef, XRBaseInteractorDestroy> _destroySamples = new();
        private readonly Dictionary<XRBaseInteractorSafeRef, XRBaseInteractorUpdate> _updateSamples = new();
        
        private RecorderContext _ctx;

        protected override void OnStartRecording(RecorderContext ctx)
        {
            base.OnStartRecording(ctx);
            _ctx = ctx;
        }

        protected override void OnStartRecordingObject(XRBaseInteractorSafeRef objSafeRef,
            RecorderContext ctx)
        {
            base.OnStartRecordingObject(objSafeRef, ctx);
            var interactable = objSafeRef.Component;
            interactable.hoverEntered.AddListener(args => OnHoverEntered(objSafeRef, args));
            interactable.hoverExited.AddListener(args => OnHoverExited(objSafeRef, args));
            interactable.selectEntered.AddListener(args => OnSelectEntered(objSafeRef, args));
            interactable.selectExited.AddListener(args => OnSelectExited(objSafeRef, args));
        }

        protected override void OnStopRecordingObject(XRBaseInteractorSafeRef objSafeRef,
            RecorderContext ctx)
        {
            base.OnStopRecordingObject(objSafeRef, ctx);
            
            var interactable = objSafeRef.Component;
            interactable.hoverEntered.RemoveListener(args => OnHoverEntered(objSafeRef, args));
            interactable.hoverExited.RemoveListener(args => OnHoverExited(objSafeRef, args));
            interactable.selectEntered.RemoveListener(args => OnSelectEntered(objSafeRef, args));
            interactable.selectExited.RemoveListener(args => OnSelectExited(objSafeRef, args));
        }
        
        protected override void OnObjectMarkedCreated(XRBaseInteractorSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            var interactable = objSafeRef.Component;
            var interactableUpdate = GetOrCreateUpdateSample(objSafeRef);
            interactableUpdate.Enabled = interactable.enabled;
            _createSamples[objSafeRef] = new XRBaseInteractorCreate { Id = GetComponentIdentifierPayload(objSafeRef) };
        }
        
        protected override void OnObjectMarkedDestroyed(XRBaseInteractorSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new XRBaseInteractorDestroy { Id = GetComponentIdentifierPayload(objSafeRef) };
        }

        private void OnHoverEntered(XRBaseInteractorSafeRef objSafeRef, HoverEnterEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactableHoverEnter = new XRBaseInteractorHoverEnter
            {
                Id = GetComponentIdentifierPayload(objSafeRef),
                InteractableCurrent = GetComponentIdentifierPayload(args.interactorObject.transform),
            };

            _ctx.CurrentRecord.RecordTimestampedManagedSample(interactableHoverEnter);
        }

        private void OnHoverExited(XRBaseInteractorSafeRef objSafeRef, HoverExitEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactableHoverExit = new XRBaseInteractorHoverExit
            {
                Id = GetComponentIdentifierPayload(objSafeRef),
                InteractableCurrent = GetComponentIdentifierPayload(args.interactorObject.transform),
            };

            _ctx.CurrentRecord.RecordTimestampedManagedSample(interactableHoverExit);
        }

        private void OnSelectEntered(XRBaseInteractorSafeRef objSafeRef, SelectEnterEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactableSelectEnter = new XRBaseInteractorSelectEnter
            {
                Id = GetComponentIdentifierPayload(objSafeRef),
                InteractableCurrent = GetComponentIdentifierPayload(args.interactorObject.transform),
            };

            _ctx.CurrentRecord.RecordTimestampedManagedSample(interactableSelectEnter);
        }

        private void OnSelectExited(XRBaseInteractorSafeRef objSafeRef, SelectExitEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactableSelectExit = new XRBaseInteractorSelectExit
            {
                Id = GetComponentIdentifierPayload(objSafeRef),
                InteractableCurrent = GetComponentIdentifierPayload(args.interactorObject.transform),
            };

            _ctx.CurrentRecord.RecordTimestampedManagedSample(interactableSelectExit);
        }

        protected override XRBaseInteractorFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = XRBaseInteractorFrameData.Pool.Get();
            frameData.AddCreateSamples(_createSamples.Values);
            frameData.AddDestroySamples(_destroySamples.Values);
            frameData.AddUpdateSamples(_updateSamples.Values);
            return frameData;
        }
        
        private XRBaseInteractorUpdate GetOrCreateUpdateSample(XRBaseInteractorSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new XRBaseInteractorUpdate {Id = GetComponentIdentifierPayload(objSafeRef)};
            _updateSamples[objSafeRef] = sample;
            return sample;
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