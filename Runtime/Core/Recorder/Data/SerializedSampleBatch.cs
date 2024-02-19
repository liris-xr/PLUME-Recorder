using ProtoBurst.Packages.ProtoBurst.Runtime;
using Unity.Collections;

namespace PLUME.Core.Recorder.Data
{
    [GenerateTestsForBurstCompatibility]
    public struct SerializedSampleBatch
    {
        public SampleTypeUrl SampleTypeUrl;
        
        public DataChunks SerializedData;

        public SerializedSampleBatch(SampleTypeUrl sampleTypeUrl, DataChunks serializedData)
        {
            SampleTypeUrl = sampleTypeUrl;
            SerializedData = serializedData;
        }
    }
}