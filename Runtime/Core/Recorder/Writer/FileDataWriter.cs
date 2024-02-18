using System;
using System.IO;
using PLUME.Core.Recorder.Data;

namespace PLUME.Core.Recorder.Writer
{
    // TODO: add metadata file
    // TODO: add delayed write
    // TODO: use memory mapped files
    public class FileDataWriter : IDataWriter, IDisposable
    {
        private readonly FileStream _stream;

        public FileDataWriter(string outputDir, string recordIdentifier)
        {
            var filePath = Path.Combine(outputDir, GenerateFileName(recordIdentifier));
            _stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        }

        private static string GenerateFileName(string recordName)
        {
            // TODO: check if file exists and prefix with number if it does
            return recordName + ".plm";
        }

        private static string GenerateMetadataFileName(string recordName)
        {
            // TODO: check if file exists and prefix with number if it does
            return recordName + ".plm.meta";
        }

        public void WriteTimelessData(DataChunks dataChunks)
        {
        }

        public void WriteTimestampedData(TimestampedDataChunks dataChunks)
        {
            // TODO: update metadata file
            var data = dataChunks.GetChunksData();
            _stream.Write(data);
        }
        
        public void Flush()
        {
            _stream.Flush();
        }

        public void Close()
        {
            _stream.Close();
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}