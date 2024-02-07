using System;
using System.Collections.Generic;
using PLUME.Recorder.Module;

namespace PLUME.Recorder
{
    public class FrameData
    {
        internal long Timestamp;
        internal readonly Dictionary<Type, IStateCollection> SamplesByType = new();

        public FrameData()
        {
        }

        public FrameData(long timestamp)
        {
            Timestamp = timestamp;
        }

        internal void SetTimestamp(long timestamp)
        {
            Timestamp = timestamp;
        }

        internal void Clear()
        {
            foreach (var (_, frameSamplesList) in SamplesByType)
            {
                frameSamplesList.Clear();
            }
        }

        public void AddSample<T>(T sample) where T : IUnityObjectState
        {
            if (!SamplesByType.TryGetValue(typeof(T), out var samplesBin))
            {
                samplesBin = new StateCollection<T>();
                SamplesByType[typeof(T)] = samplesBin;
            }

            ((StateCollection<T>)samplesBin).Add(sample);
        }
        
        public void AddSamples<T>(ReadOnlyMemory<T> samples) where T : IUnityObjectState
        {
            AddSamples(samples.Span);
        }
        
        public void AddSamples<T>(ReadOnlySpan<T> samples) where T : IUnityObjectState
        {
            if (!SamplesByType.TryGetValue(typeof(T), out var samplesBin))
            {
                samplesBin = new StateCollection<T>();
                SamplesByType[typeof(T)] = samplesBin;
            }

            ((StateCollection<T>)samplesBin).AddRange(samples);
        }
    }
}