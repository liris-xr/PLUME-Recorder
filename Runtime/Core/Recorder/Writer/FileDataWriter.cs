using System;
using System.IO;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Internal;
using K4os.Compression.LZ4.Streams;
using UnityEngine;

namespace PLUME.Core.Recorder.Writer
{
    // TODO: add metadata file
    // TODO: add delayed write
    // TODO: use memory mapped files
    public class FileDataWriter : IDataWriter, IDisposable
    {
        private readonly Stream _stream;
        private readonly Stream _metadataStream;

        private Sample.RecordMetadata _metadata;
        private bool _isSequential;
        private ulong _sampleCount;
        private long _lastWrittenTimestamp;
        
        public FileDataWriter(Record record)
        {
            var outputDir = Application.persistentDataPath;

            GenerateFilePath(outputDir, record.Metadata, out var filePath, out var metadataFilePath);

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
            _metadataStream = File.Create(metadataFilePath);

            _metadata = new Sample.RecordMetadata
            {
                Name = record.Metadata.Name,
                ExtraMetadata = record.Metadata.ExtraMetadata,
                Sequential = true,
                RecorderVersion = PlumeRecorder.Version,
                CreatedAt = Timestamp.FromDateTime(record.Metadata.StartTime),
            };
        }

        private static void GenerateFilePath(string outputDir, RecordMetadata recordMetadata,
            out string filePath, out string metadataPath)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
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
            _sampleCount += (ulong)dataChunks.ChunksCount;
            UpdateMetadata();
        }

        public void WriteTimestampedData(DataChunksTimestamped dataChunks)
        {
            if (dataChunks.IsEmpty()) return;
            _stream.Write(dataChunks.GetDataSpan());
            var lastTimestamp = dataChunks.Timestamps[^1];
            _isSequential &= lastTimestamp >= _lastWrittenTimestamp;
            _lastWrittenTimestamp = lastTimestamp;
            _sampleCount += (ulong)dataChunks.ChunksCount;
            UpdateMetadata();
        }

        private void UpdateMetadata()
        {
            _metadataStream.SetLength(0);
            _metadataStream.Position = 0;
            _metadata.SamplesCount = _sampleCount;
            _metadata.Sequential = _isSequential;
            _metadata.Duration = (ulong)Math.Max(0, _lastWrittenTimestamp);
            _metadata.WriteDelimitedTo(_metadataStream);
        }

        public void Flush()
        {
            _stream.Flush();
            _metadataStream.Flush();
        }

        public void Close()
        {
            _stream.Close();
            _metadataStream.Close();
        }

        public void Dispose()
        {
            _stream.Dispose();
            _metadataStream.Dispose();
        }
    }
}