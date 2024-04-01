using System;
using System.IO;
using System.Linq;
using Google.Protobuf;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Internal;
using K4os.Compression.LZ4.Streams;
using PLUME.Sample;
using UnityEngine;

namespace PLUME.Core.Recorder.Writer
{
    // TODO: add metadata file
    // TODO: add delayed write
    // TODO: use memory mapped files
    public class FileDataWriter : IDataWriter, IDisposable
    {
        private readonly Stream _stream;
        private readonly Stream _metaStream;
        private readonly CodedOutputStream _metaCodedOutputStream;
        
        private readonly Sample.RecordMetadata _metadata;
        private readonly RecordMetrics _metrics;
        
        public FileDataWriter(Record record)
        {
            var outputDir = Application.persistentDataPath;

            GenerateFilePath(outputDir, record.Metadata, out var filePath, out var metaFilePath);
            
            Logger.Log($"Record will be saved to '{filePath}'.");

            PinnedMemory.MaxPooledSize = 0;

            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.EmbeddedLinuxArm32 ||
                Application.platform == RuntimePlatform.QNXArm32)
            {
                LZ4Codec.Enforce32 = true;
            }
            else
            {
                LZ4Codec.Enforce32 = false;
            }

            _stream = LZ4Stream.Encode(File.Create(filePath), LZ4Level.L00_FAST);
            _metaStream = File.Create(metaFilePath);
            _metaCodedOutputStream = new CodedOutputStream(_metaStream);
            
            _metadata = record.Metadata.ToPayload();
            _metrics = new RecordMetrics
            {
                IsSequential = true
            };
            
            UpdateMetaFile();
        }

        private static void GenerateFilePath(string outputDir, RecordMetadata recordMetadata,
            out string filePath, out string metadataPath)
        {
            var invalidChars = Path.GetInvalidFileNameChars().ToList();
            invalidChars.Add(' ');
            
            var name = recordMetadata.Name;
            var safeName = new string(name.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());

            var formattedDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-sszz");
            var filenameBase = $"{safeName}_{formattedDateTime}";
            const string fileExtension = ".plm";

            var i = 0;

            do
            {
                var suffix = i == 0 ? "" : "_" + i;
                filePath = Path.Join(outputDir, filenameBase + suffix + fileExtension);
                ++i;
            } while (File.Exists(filePath));

            metadataPath = filePath + ".meta";
        }

        public void WriteTimelessData(DataChunks dataChunks)
        {
            if (dataChunks.IsEmpty()) return;
            _stream.Write(dataChunks.GetDataSpan());
            _metrics.NSamples += (ulong)dataChunks.ChunksCount;
            UpdateMetaFile();
        }

        public void WriteTimestampedData(DataChunksTimestamped dataChunks)
        {
            if (dataChunks.IsEmpty()) return;
            _stream.Write(dataChunks.GetDataSpan());
            var timestamp = Math.Max(0, dataChunks.Timestamps[^1]);
            _metrics.IsSequential &= timestamp >= _metrics.Duration;
            _metrics.Duration = timestamp;
            _metrics.NSamples += (ulong)dataChunks.ChunksCount;
            UpdateMetaFile();
        }

        private void UpdateMetaFile()
        {
            _metaStream.SetLength(0);
            _metaStream.Position = 0;
            _metaCodedOutputStream.WriteLength(_metadata.CalculateSize());
            _metadata.WriteTo(_metaCodedOutputStream);
            _metaCodedOutputStream.WriteLength(_metrics.CalculateSize());
            _metrics.WriteTo(_metaCodedOutputStream);
            _metaCodedOutputStream.Flush();
            _metaStream.Flush();
        }

        public void Flush()
        {
            _stream.Flush();
            _metaStream.Flush();
            _metaCodedOutputStream.Flush();
        }

        public void Close()
        {
            _stream.Close();
            _metaStream.Close();
        }

        public void Dispose()
        {
            _stream.Dispose();
            _metaStream.Dispose();
            _metaCodedOutputStream.Dispose();
        }
    }
}