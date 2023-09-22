using System;
using System.IO;
using System.IO.Compression;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using PLUME.Sample;
using UnityEngine;

namespace PLUME
{
    public class RecordWriter : IDisposable
    {
        private readonly DateTime _createdAt;
        private readonly ExtraMetadata[] _extraMetadata;
        private readonly string _recordIdentifier;

        private readonly string _tmpFilepath;
        private readonly FileStream _tmpStream;

        private readonly string _filepath;

        private readonly bool _leaveOpen;

        private long _samplesCount;
        private long _duration;

        private bool _closed;

        private bool _useCompression;

        public RecordWriter(string filepath, string recordIdentifier, ExtraMetadata[] extraMetadata,
            bool useCompression, int bufferSize = 4096,
            bool leaveOpen = false)
        {
            _filepath = filepath;
            _createdAt = DateTime.UtcNow;
            _extraMetadata = extraMetadata;
            _recordIdentifier = recordIdentifier;
            _tmpFilepath = GetTmpFilePath();
            _tmpStream = File.Create(_tmpFilepath, bufferSize);
            _leaveOpen = leaveOpen;
            _useCompression = useCompression;

            WriteMetadata(Recorder.Version, _createdAt, recordIdentifier, false, extraMetadata);
        }

        private static string GetTmpFilePath()
        {
            return Path.Combine(Application.persistentDataPath, $"plume_tmp_{System.Guid.NewGuid().ToString()}.tmp");
        }

        private void WriteMetadata(RecorderVersion recorderVersion, DateTime creationTime, string recordIdentifier,
            bool sequential, ExtraMetadata[] extraMetadata, long sampleCount = -1, long duration = -1)
        {
            var metadata = new RecordMetadata();
            metadata.RecorderVersion = recorderVersion;
            metadata.CreatedAt = Timestamp.FromDateTime(creationTime.ToUniversalTime());
            metadata.Identifier = recordIdentifier;
            metadata.Sequential = sequential;

            if (extraMetadata != null)
            {
                foreach (var extraMetadataEntry in extraMetadata)
                {
                    metadata.ExtraMetadata.Add(extraMetadataEntry.Key, extraMetadataEntry.Value);
                }
            }

            // 0 values are not serialized. We ensure that the values != 0 to keep a constant message size for overwriting placeholder values at the end of the record.
            if (sampleCount == 0)
                sampleCount = -1;
            if (duration == 0)
                duration = -1;

            metadata.SamplesCount = sampleCount;
            metadata.Duration = duration;

            metadata.WriteDelimitedTo(_tmpStream);
        }

        public void WriteSample(PackedSample sample)
        {
            sample.WriteDelimitedTo(_tmpStream);
            _samplesCount++;
            _duration = Math.Max(_duration, (long)sample.Header.Time);
        }

        public void Close()
        {
            _tmpStream.Flush(true);

            if (_closed)
                return;

            // Overwrite samples count and duration placeholders.
            _tmpStream.Seek(0, SeekOrigin.Begin);
            WriteMetadata(Recorder.Version, _createdAt, _recordIdentifier, false, _extraMetadata, _samplesCount,
                _duration);

            if (_useCompression)
            {
                try
                {
                    var outputStream = new GZipStream(File.Create(_filepath), CompressionMode.Compress);
                    _tmpStream.Seek(0, SeekOrigin.Begin);
                    _tmpStream.CopyTo(outputStream);
                    _tmpStream.Close();

                    if (!_leaveOpen)
                        outputStream.Close();

                    File.Delete(_tmpFilepath);
                }
                catch (IOException e)
                {
                    Debug.LogError($"Failed to write final output file. Keeping temporary files. {e}");
                }
            }
            else
            {
                try
                {
                    _tmpStream.Close();
                    File.Move(_tmpFilepath, _filepath);
                }
                catch (IOException e)
                {
                    Debug.LogError($"Failed to write final output file. Keeping temporary files. {e}");
                }
            }

            _closed = true;
        }

        public void Dispose()
        {
            Close();
        }

        private string GetSizeString(long length)
        {
            const long kb = 1024L;
            const long mb = kb * 1024L;
            const long gb = mb * 1024L;
            const long tb = gb * 1024L;

            double size = length;
            var suffix = "B";

            switch (length)
            {
                case >= tb:
                    size = Math.Round((double)length / tb, 2);
                    suffix = "TB";
                    break;
                case >= gb:
                    size = Math.Round((double)length / gb, 2);
                    suffix = "GB";
                    break;
                case >= mb:
                    size = Math.Round((double)length / mb, 2);
                    suffix = "MB";
                    break;
                case >= kb:
                    size = Math.Round((double)length / kb, 2);
                    suffix = "KB";
                    break;
            }

            return $"{size}{suffix}";
        }
    }
}