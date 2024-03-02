namespace PLUME.Base.Module.Unity.Audio
{
    public readonly struct AudioSamples
    {
        public readonly ulong Timestamp;
        public readonly float[] Samples;
        public readonly int ChannelCount;
        
        public AudioSamples(ulong timestamp, float[] samples, int channelCount)
        {
            Timestamp = timestamp;
            Samples = samples;
            ChannelCount = channelCount;
        }
    }
}