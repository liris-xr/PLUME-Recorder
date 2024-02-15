using System;
using System.Collections.Generic;
using System.IO;
using PLUME.Core.Recorder.Data;

namespace PLUME.Core.Recorder.Writer
{
    // TODO: add metadata file
    // TODO: add delayed write
    public class FileDataWriter : IDataWriter, IDisposable
    {
        private readonly FileStream _stream;

        public FileDataWriter(string outputDir, string recordIdentifier)
        {
            var filePath = Path.Combine(outputDir, GenerateFileName(recordIdentifier));
            _stream = File.Create(filePath);
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

        public void WriteTimestampedData(DataChunks dataChunks, List<long> timestamps)
        {
            // TODO: update metadata file
            _stream.Write(dataChunks.GetAllChunksData());
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