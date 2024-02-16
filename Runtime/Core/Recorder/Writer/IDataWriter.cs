using System.Collections.Generic;
using PLUME.Core.Recorder.Data;

namespace PLUME.Core.Recorder.Writer
{
    public interface IDataWriter
    {
        public void WriteTimelessData(DataChunks dataChunks);

        public void WriteTimestampedData(DataChunks dataChunks, List<long> timestamps);

        void Flush();
        
        public void Close();
    }
}