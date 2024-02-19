using System;
using System.IO;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Internal;
using K4os.Compression.LZ4.Streams;
using PLUME.Core.Recorder.Data;
using UnityEngine;

namespace PLUME.Core.Recorder.Writer
{
    // TODO: add metadata file
    // TODO: add delayed write
    // TODO: use memory mapped files
    public class FileDataWriter : IDataWriter, IDisposable
    {
        private readonly Stream _stream;

        public FileDataWriter(RecordIdentifier recordIdentifier)
        {
            var outputDir = Application.persistentDataPath;
            
            var filePath = Path.Combine(outputDir, GenerateFileName(recordIdentifier));
            
            PinnedMemory.MaxPooledSize = 0;
            // TODO: only enable on 32 bit systems or for IL2CPP
            // LZ4Codec.Enforce32 = true;
            _stream = LZ4Stream.Encode(File.Create(filePath), LZ4Level.L00_FAST);
        }

        private static string GenerateFileName(RecordIdentifier recordIdentifier)
        {
            // TODO: check if file exists and prefix with number if it does
            return recordIdentifier.Identifier + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".plm";
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
            // TODO: create a local buffer
            // TODO: update metadata file
            _stream.Write(dataChunks.GetDataSpan());
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
            _stream.Dispose();
        }
    }
}