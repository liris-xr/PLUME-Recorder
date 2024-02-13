using System;
using System.IO;
using System.IO.Compression;

namespace PLUME.Core.Writer
{
    // TODO: add metadata file
    // TODO: add delayed write
    public class FileDataWriter : IDataWriter, IDisposable
    {
        private readonly GZipStream _stream;

        public FileDataWriter(string outputDir, string recordIdentifier)
        {
            var filePath = Path.Combine(outputDir, GenerateFileName(recordIdentifier));
            _stream = new GZipStream(File.Create(filePath), CompressionLevel.Optimal);
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
        
        public void WriteTimelessData(ReadOnlySpan<byte> data)
        {
        }
        
        public void WriteTimelessData(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths)
        {
        }

        public void WriteTimestampedData(ReadOnlySpan<byte> data, long timestamp)
        {
            // TODO: update metadata file
            _stream.Write(data);
        }

        public void WriteTimestampedData(ReadOnlySpan<byte> data, ReadOnlySpan<int> lengths, ReadOnlySpan<long> timestamps)
        {
            // TODO: update metadata file
            _stream.Write(data);
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