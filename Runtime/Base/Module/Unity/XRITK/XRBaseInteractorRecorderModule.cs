#if USE_XRITK_2 || USE_XRITK_3
using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.XRITK;
using UnityEngine.Scripting;
using UnityEngine.XR.Interaction.Toolkit;
using static PLUME.Core.Utils.SampleUtils;

#if USE_XRITK_2
using XRBaseInteractorSafeRef =
    PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.XR.Interaction.Toolkit.XRBaseInteractor>;
#elif USE_XRITK_3
using XRBaseInteractorSafeRef =
    PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>;
#endif

namespace PLUME.Base.Module.Unity.XRITK
{
    [Preserve]
#if USE_XRITK_2
    public class XRBaseInteractorRecorderModule : ComponentRecorderModule<UnityEngine.XR.Interaction.Toolkit.XRBaseInteractor, XRBaseInteractorFrameData>
#elif USE_XRITK_3
    public class XRBaseInteractorRecorderModule : ComponentRecorderModule<
        UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor, XRBaseInteractorFrameData>
#endif
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
            var interactor = objSafeRef.Component;
            interactor.hoverEntered.AddListener(args => OnHoverEntered(objSafeRef, args));
            interactor.hoverExited.AddListener(args => OnHoverExited(objSafeRef, args));
            interactor.selectEntered.AddListener(args => OnSelectEntered(objSafeRef, args));
            interactor.selectExited.AddListener(args => OnSelectExited(objSafeRef, args));
        }

        protected override void OnStopRecordingObject(XRBaseInteractorSafeRef objSafeRef,
            RecorderContext ctx)
        {
            base.OnStopRecordingObject(objSafeRef, ctx);

            var interactor = objSafeRef.Component;
            interactor.hoverEntered.RemoveListener(args => OnHoverEntered(objSafeRef, args));
            interactor.hoverExited.RemoveListener(args => OnHoverExited(objSafeRef, args));
            interactor.selectEntered.RemoveListener(args => OnSelectEntered(objSafeRef, args));
            interactor.selectExited.RemoveListener(args => OnSelectExited(objSafeRef, args));
        }

        protected override void OnObjectMarkedCreated(XRBaseInteractorSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            var interactor = objSafeRef.Component;
            var interactorUpdate = GetOrCreateUpdateSample(objSafeRef);
            interactorUpdate.Enabled = interactor.enabled;
            _createSamples[objSafeRef] = new XRBaseInteractorCreate { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(XRBaseInteractorSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new XRBaseInteractorDestroy
                { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        private void OnHoverEntered(XRBaseInteractorSafeRef objSafeRef, HoverEnterEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactorHoverEnter = new XRBaseInteractorHoverEnter
            {
                Component = GetComponentIdentifierPayload(objSafeRef),
                Interactable = GetComponentIdentifierPayload(args.interactorObject.transform),
            };

            _ctx.CurrentRecord.RecordTimestampedManagedSample(interactorHoverEnter);
        }

        private void OnHoverExited(XRBaseInteractorSafeRef objSafeRef, HoverExitEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactorHoverExit = new XRBaseInteractorHoverExit
            {
                Component = GetComponentIdentifierPayload(objSafeRef),
                Interactable = GetComponentIdentifierPayload(args.interactorObject.transform),
            };

            _ctx.CurrentRecord.RecordTimestampedManagedSample(interactorHoverExit);
        }

        private void OnSelectEntered(XRBaseInteractorSafeRef objSafeRef, SelectEnterEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactorSelectEnter = new XRBaseInteractorSelectEnter
            {
                Component = GetComponentIdentifierPayload(objSafeRef),
                Interactable = GetComponentIdentifierPayload(args.interactorObject.transform),
            };

            _ctx.CurrentRecord.RecordTimestampedManagedSample(interactorSelectEnter);
        }

        private void OnSelectExited(XRBaseInteractorSafeRef objSafeRef, SelectExitEventArgs args)
        {
            if (!_ctx.IsRecording)
                return;

            var interactorSelectExit = new XRBaseInteractorSelectExit
            {
                Component = GetComponentIdentifierPayload(objSafeRef),
                Interactable = GetComponentIdentifierPayload(args.interactorObject.transform),
            };

            _ctx.CurrentRecord.RecordTimestampedManagedSample(interactorSelectExit);
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
            sample = new XRBaseInteractorUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
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