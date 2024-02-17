using System.Collections.Generic;
using PLUME.Core.Recorder.Data;

namespace PLUME.Core.Recorder.Writer
{
    public interface IDataWriter
    {
        public void WriteTimelessData(DataChunks dataChunks);

        public void WriteTimestampedData(TimestampedDataChunks dataChunks);

        void Flush();
        
        public void Close();
    }
}