using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Google.Protobuf;
using PLUME.Sample;
using UnityEngine;

namespace PLUME
{
    public class RecordWriter : IDisposable
    {
        private readonly DateTime _createdAt;
        private readonly string _recordIdentifier;

        private readonly string _tmpFilepath;
        private readonly FileStream _tmpStream;

        private readonly string _filepath;
        private readonly Stream _customOutputStream;

        private readonly bool _leaveOpen;

        private int _samplesCount;
        private ulong _duration;

        private bool _closed;

        private bool _useCompression;

        public RecordWriter(string filepath, string recordIdentifier, bool useCompression, int bufferSize = 4096,
            bool leaveOpen = false)
        {
            _filepath = filepath;
            _createdAt = DateTime.UtcNow;
            _recordIdentifier = recordIdentifier;
            _tmpFilepath = GetTmpFilePath();
            _tmpStream = File.Create(_tmpFilepath, bufferSize);
            _leaveOpen = leaveOpen;
            _useCompression = useCompression;
            WriteFileSignature(_tmpStream);
        }

        private static void WriteFileSignature(Stream stream)
        {
            var fileSignature = Encoding.ASCII.GetBytes("PLUME_RAW");
            stream.Write(fileSignature);
        }

        private static string GetTmpFilePath()
        {
            return Path.Combine(Application.persistentDataPath, $"plume_tmp_{System.Guid.NewGuid().ToString()}.tmp");
        }

        public void WriteSample(PackedSample sample)
        {
            sample.WriteDelimitedTo(_tmpStream);
            _samplesCount++;
            _duration = Math.Max(_duration, sample.Header.Time);
        }

        public void Close()
        {
            _tmpStream.Flush(true);
            
            if (_closed)
                return;

            if (_customOutputStream != null)
            {
                try
                {
                    _tmpStream.CopyTo(_customOutputStream);
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
                    if (_useCompression)
                    {
                        using var outputStream = File.Create(_filepath);
                        using var gzip = new GZipStream(outputStream, CompressionMode.Compress);
                        _tmpStream.Position = 0;
                        _tmpStream.CopyTo(gzip);
                        _tmpStream.Close();
                        gzip.Close();
                        File.Delete(_tmpFilepath);
                    }
                    else
                    {
                        _tmpStream.Close();
                        File.Move(_tmpFilepath, _filepath);
                    }
                    
                    var info = new FileInfo(_filepath);
                    Debug.Log($"File size: {GetSizeString(info.Length)}");
                }
                catch (IOException e)
                {
                    Debug.LogError($"Failed to write final output file. Keeping temporary files. {e}");
                }
            }

            _closed = true;
        }

        private static void CopyBytes(Stream srcStream, Stream outStream, long srcStart, long nBytes)
        {
            var readBytes = 0;
            var buffer = new byte[64 * 1024];

            do
            {
                var toRead = Math.Min(nBytes - readBytes, buffer.Length);
                srcStream.Seek(srcStart, SeekOrigin.Begin);
                var readNow = srcStream.Read(buffer, 0, (int)toRead);
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