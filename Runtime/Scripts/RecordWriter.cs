using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using PLUME.Sample;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace PLUME
{
    public class RecordWriter : IDisposable
    {
        private readonly DateTime _createdAt;
        private readonly string _recordIdentifier;

        private readonly string _tmpFilepath;
        private readonly FileStream _tmpStream;
        private readonly List<TemporaryStreamSampleInfo> _tmpSamplesInfos = new();

        private readonly string _filepath;
        private readonly Stream _customOutputStream;

        private readonly bool _leaveOpen;

        private int _samplesCount;
        private ulong _duration;

        private bool _closed;

        public RecordWriter(string filepath, string recordIdentifier, bool leaveOpen = false)
        {
            _filepath = filepath;
            _createdAt = DateTime.UtcNow;
            _recordIdentifier = recordIdentifier;
            _tmpFilepath = Path.Combine(Directory.GetCurrentDirectory(), GenerateTmpFileName());
            _tmpStream = new FileStream(_tmpFilepath, FileMode.CreateNew, FileAccess.ReadWrite);
            _leaveOpen = leaveOpen;
        }

        public RecordWriter(Stream outputStream, string recordIdentifier, bool leaveOpen = false)
        {
            _createdAt = DateTime.UtcNow;
            _recordIdentifier = recordIdentifier;
            _tmpFilepath = Path.Combine(Directory.GetCurrentDirectory(), GenerateTmpFileName());
            _tmpStream = new FileStream(_tmpFilepath, FileMode.CreateNew, FileAccess.ReadWrite);
            _customOutputStream = outputStream;
            _leaveOpen = leaveOpen;
        }

        private static string GenerateTmpFileName()
        {
            return $"plume_tmp_{Guid.NewGuid().ToString()}.tmp";
        }

        public void WriteSample(PackedSample sample)
        {
            var startPos = _tmpStream.Position;
            sample.WriteDelimitedTo(_tmpStream);
            var endPos = _tmpStream.Position;

            _tmpSamplesInfos.Add(new TemporaryStreamSampleInfo
            {
                seq = sample.Header.Seq,
                timestamp = sample.Header.Time,
                byteStartPos = startPos,
                byteEndPos = endPos
            });

            _samplesCount++;
            _duration = Math.Max(_duration, sample.Header.Time);
        }

        public void Close()
        {
            if (_closed)
                return;

            Profiler.BeginSample("Merging tmp files");

            var header = new RecordHeader();
            header.Version = Plume.Version;
            header.CreatedAt = Timestamp.FromDateTime(_createdAt);
            header.Identifier = _recordIdentifier;
            header.SamplesCount = _samplesCount;
            header.Duration = _duration;
            header.RenderingPipeline = GraphicsSettings.currentRenderPipeline == null
                ? "Built-in Render Pipeline"
                : GraphicsSettings.currentRenderPipeline.name;
            header.ExtraMetadata = "";

            _tmpSamplesInfos.Sort(new TemporaryStreamSampleInfoComparer());
            
            if (_customOutputStream != null)
            {
                try
                {
                    // TODO: write samples in order
                    
                    header.WriteDelimitedTo(_customOutputStream);
                    
                    foreach (var tmpSampleInfo in _tmpSamplesInfos)
                    {
                        CopyBytes(_tmpStream, _customOutputStream, tmpSampleInfo.byteStartPos, tmpSampleInfo.byteEndPos - tmpSampleInfo.byteStartPos);
                    }
                    
                    _tmpStream.Close();
                    if (!_leaveOpen)
                        _customOutputStream.Close();
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
                    Debug.Log("Merging temporary files and compressing data.");
                    using var dstStream =
                        new GZipStream(new FileStream(_filepath, FileMode.CreateNew, FileAccess.Write),
                            CompressionLevel.Optimal);
                    header.WriteDelimitedTo(dstStream);
                    
                    foreach (var tmpSampleInfo in _tmpSamplesInfos)
                    {
                        CopyBytes(_tmpStream, dstStream, tmpSampleInfo.byteStartPos, tmpSampleInfo.byteEndPos - tmpSampleInfo.byteStartPos);
                    }
                    
                    _tmpStream.Close();
                    Debug.Log($"Final output file size is {GetSizeString(dstStream.BaseStream.Length)}.");
                    File.Delete(_tmpFilepath);
                }
                catch (IOException e)
                {
                    Debug.LogError($"Failed to write final output file. Keeping temporary files. {e}");
                }
            }

            Profiler.EndSample();

            _closed = true;
        }

        private static void CopyBytes(Stream srcStream, Stream outStream, long srcStart, long nBytes)
        {
            var readBytes = 0;
            var buffer = new byte[64*1024];
            
            do
            {
                var toRead = Math.Min(nBytes - readBytes, buffer.Length);
                srcStream.Seek(srcStart, SeekOrigin.Begin);
                var readNow = srcStream.Read(buffer, 0, (int) toRead);
                if (readNow == 0)
                    break; // End of stream
                outStream.Write(buffer, 0, readNow);
                readBytes += readNow;
            } while (readBytes < nBytes);
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
                    size = Math.Round((double) length / tb, 2);
                    suffix = "TB";
                    break;
                case >= gb:
                    size = Math.Round((double) length / gb, 2);
                    suffix = "GB";
                    break;
                case >= mb:
                    size = Math.Round((double) length / mb, 2);
                    suffix = "MB";
                    break;
                case >= kb:
                    size = Math.Round((double) length / kb, 2);
                    suffix = "KB";
                    break;
            }

            return $"{size}{suffix}";
        }
    }
    
    public class TemporaryStreamSampleInfo
    {
        public ulong seq;
        public ulong timestamp;
        public long byteStartPos;
        public long byteEndPos;
    }

    public class TemporaryStreamSampleInfoComparer : IComparer<TemporaryStreamSampleInfo>
    {
        public int Compare(TemporaryStreamSampleInfo x, TemporaryStreamSampleInfo y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            var seqComparison = x.seq.CompareTo(y.seq);
            return seqComparison != 0 ? seqComparison : x.timestamp.CompareTo(y.timestamp);
        }
    }
}