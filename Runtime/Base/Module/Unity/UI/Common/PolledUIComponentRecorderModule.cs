using System.Collections.Generic;
using Google.Protobuf;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;
using UnityEngine;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Common
{
    /// <summary>
    /// Records a Unity UI component by polling it once per frame and emitting an update sample holding only the
    /// properties that changed since the last recorded state.
    /// <para>
    /// Polling is used instead of setter hooks because most UI state is written from inside Unity's own assemblies
    /// (layout rebuilds, Selectable transitions, CanvasRenderer tweens, Animator bindings), where hooks injected into
    /// user code never run. The first poll of a component, or its creation, records every property.
    /// </para>
    /// </summary>
    /// <typeparam name="TC">Recorded component type. Derived component types are recorded too.</typeparam>
    /// <typeparam name="TState">Snapshot of the recorded properties, compared frame to frame.</typeparam>
    public abstract class PolledUIComponentRecorderModule<TC, TState, TCreate, TDestroy, TUpdate, TEvent> :
        ComponentRecorderModule<TC, UIComponentFrameData<TCreate, TDestroy, TUpdate, TEvent>>
        where TC : Component
        where TState : struct
        where TCreate : IMessage
        where TDestroy : IMessage
        where TUpdate : IMessage
        where TEvent : IMessage
    {
        private readonly Dictionary<IComponentSafeRef<TC>, TState> _lastStates = new();

        private readonly List<TCreate> _createSamples = new();
        private readonly List<TDestroy> _destroySamples = new();
        private readonly List<TUpdate> _updateSamples = new();
        private readonly List<TEvent> _eventSamples = new();

        private UpdateSampleBuilder _builder;

        /// <summary>Whether creation and destruction samples are emitted. Disabled for modules recording a facet
        /// of a component whose lifecycle is already recorded by another module.</summary>
        protected virtual bool RecordLifecycle => true;

        protected abstract TCreate CreateCreateSample(ComponentIdentifier id);

        protected abstract TDestroy CreateDestroySample(ComponentIdentifier id);

        protected abstract TUpdate CreateUpdateSample(ComponentIdentifier id);

        protected abstract TState ReadState(TC component);

        /// <summary>
        /// Writes into <c>sample.Get()</c> every property of <paramref name="current"/> that differs from
        /// <paramref name="previous"/>, or every property when <paramref name="hasPrevious"/> is false. The update
        /// sample is only allocated on the first <c>Get()</c>, so an unchanged component allocates nothing.
        /// </summary>
        protected abstract void WriteChanges(bool hasPrevious, in TState previous, in TState current,
            UpdateSampleBuilder sample);

        /// <summary>Lazily creates the update sample of the component being polled.</summary>
        protected sealed class UpdateSampleBuilder
        {
            private readonly PolledUIComponentRecorderModule<TC, TState, TCreate, TDestroy, TUpdate, TEvent> _module;
            internal IComponentSafeRef<TC> SafeRef;
            internal TUpdate Sample;
            internal bool HasSample;

            internal UpdateSampleBuilder(
                PolledUIComponentRecorderModule<TC, TState, TCreate, TDestroy, TUpdate, TEvent> module)
            {
                _module = module;
            }

            public TUpdate Get()
            {
                if (HasSample)
                    return Sample;

                Sample = _module.CreateUpdateSample(GetComponentIdentifierPayload(SafeRef));
                HasSample = true;
                return Sample;
            }
        }

        protected void AddEventSample(TEvent sample) => _eventSamples.Add(sample);

        protected override void OnObjectMarkedCreated(IComponentSafeRef<TC> objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            if (RecordLifecycle)
                _createSamples.Add(CreateCreateSample(GetComponentIdentifierPayload(objSafeRef)));

            // Record the full state now rather than at the next poll, so the update lands in the creation frame.
            _lastStates.Remove(objSafeRef);
            PollComponent(objSafeRef);
        }

        protected override void OnObjectMarkedDestroyed(IComponentSafeRef<TC> objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);

            if (RecordLifecycle)
                _destroySamples.Add(CreateDestroySample(GetComponentIdentifierPayload(objSafeRef)));
        }

        protected override void OnStopRecordingObject(IComponentSafeRef<TC> objSafeRef, RecorderContext ctx)
        {
            base.OnStopRecordingObject(objSafeRef, ctx);
            _lastStates.Remove(objSafeRef);
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            _lastStates.Clear();
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
            _eventSamples.Clear();
        }

        protected override UIComponentFrameData<TCreate, TDestroy, TUpdate, TEvent> CollectFrameData(
            FrameInfo frameInfo, RecorderContext ctx)
        {
            var recorded = RecordedComponents;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < recorded.Count; i++)
                PollComponent(recorded[i]);

            var frameData = UIComponentFrameData<TCreate, TDestroy, TUpdate, TEvent>.Pool.Get();
            frameData.AddCreateSamples(_createSamples);
            frameData.AddDestroySamples(_destroySamples);
            frameData.AddUpdateSamples(_updateSamples);
            frameData.AddEventSamples(_eventSamples);
            return frameData;
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
            _eventSamples.Clear();
        }

        private void PollComponent(IComponentSafeRef<TC> objSafeRef)
        {
            var component = objSafeRef.Component;

            // Destroyed but not yet removed from the recorded objects.
            if (component == null)
                return;

            var current = ReadState(component);
            var hasPrevious = _lastStates.TryGetValue(objSafeRef, out var previous);

            _builder ??= new UpdateSampleBuilder(this);
            _builder.SafeRef = objSafeRef;
            _builder.HasSample = false;
            _builder.Sample = default;

            WriteChanges(hasPrevious, previous, current, _builder);

            if (_builder.HasSample)
                _updateSamples.Add(_builder.Sample);

            _builder.SafeRef = null;
            _builder.Sample = default;
            _lastStates[objSafeRef] = current;
        }
    }
}
