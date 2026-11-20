using System.Collections.Generic;
using Google.Protobuf;
using PLUME.Core.Recorder.Module.Frame;

namespace PLUME.Base.Module.Unity.UI.Common
{
    /// <summary>
    /// Frame data holding the create, destroy and update samples of one UI component type, plus an optional batch of
    /// event samples (e.g. clicks). Each list is homogeneous, as required by <see cref="FrameDataWriter.WriteManagedBatch{T}"/>.
    /// </summary>
    public class UIComponentFrameData<TCreate, TDestroy, TUpdate, TEvent> :
        PooledFrameData<UIComponentFrameData<TCreate, TDestroy, TUpdate, TEvent>>
        where TCreate : IMessage
        where TDestroy : IMessage
        where TUpdate : IMessage
        where TEvent : IMessage
    {
        public static readonly FrameDataPool<UIComponentFrameData<TCreate, TDestroy, TUpdate, TEvent>> Pool = new();

        private readonly List<TCreate> _createSamples = new();
        private readonly List<TDestroy> _destroySamples = new();
        private readonly List<TUpdate> _updateSamples = new();
        private readonly List<TEvent> _eventSamples = new();

        public void AddCreateSamples(IEnumerable<TCreate> samples) => _createSamples.AddRange(samples);

        public void AddDestroySamples(IEnumerable<TDestroy> samples) => _destroySamples.AddRange(samples);

        public void AddUpdateSamples(IEnumerable<TUpdate> samples) => _updateSamples.AddRange(samples);

        public void AddEventSamples(IEnumerable<TEvent> samples) => _eventSamples.AddRange(samples);

        public override void Serialize(FrameDataWriter frameDataWriter)
        {
            frameDataWriter.WriteManagedBatch(_createSamples);
            frameDataWriter.WriteManagedBatch(_destroySamples);
            frameDataWriter.WriteManagedBatch(_updateSamples);
            frameDataWriter.WriteManagedBatch(_eventSamples);
        }

        public override void Clear()
        {
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
            _eventSamples.Clear();
        }
    }
}
