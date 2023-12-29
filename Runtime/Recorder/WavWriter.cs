using System;
using System.IO;
using System.Text;

namespace PLUME
{
    public class WavWriter : IDisposable
    {
        private const int HeaderSize = 44;

        private const int BytesPerSample = 2; // 16 bit
        private const float RescaleFactor = 32767; //to convert float to Int16
        
        private Stream _stream;
        private BinaryWriter _writer;

        private int _sampleRate; // in Hz
        private int _channels; // 1 for mono, 2 for stereo, etc

        private int _nPerChannelSampleCount;
        
        public WavWriter(Stream stream, int sampleRate, int channels)
        {
            if (!stream.CanSeek)
            {
                throw new ArgumentException("Stream should be seekable", nameof(stream));
            }
            
            _stream = stream;
            _writer = new BinaryWriter(stream);

            _sampleRate = sampleRate;
            _channels = channels;
        }
        
        public double GetDuration()
        {
            return _nPerChannelSampleCount / (double) _sampleRate;
        }
        
        public void WriteWaveData(float[] buffer)
        {
            if (_stream.Length > HeaderSize)
            {
                _stream.Seek(0, SeekOrigin.End);
            }
            else
            {
                _stream.SetLength(HeaderSize);
                _stream.Position = HeaderSize;
            }

            // rescale
            var floats = Array.ConvertAll(buffer, x => (short)(x * RescaleFactor));

            // Copy to bytes
            var result = new byte[floats.Length * sizeof(short)];
            Buffer.BlockCopy(floats, 0, result, 0, result.Length);

            // write to stream
            _stream.Write(result, 0, result.Length);

            _nPerChannelSampleCount += buffer.Length / _channels;
            
            // Update Header
            UpdateHeader();
        }
        
        private void UpdateHeader()
        {
            _writer.Seek(0, SeekOrigin.Begin);
            
            _writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            // Size of the overall file - 8 bytes (for the "RIFF" string and this value).
            _writer.Write((int)(_writer.BaseStream.Length - 8));
            // File type header
            _writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            
            // Format chunk marker
            _writer.Write(Encoding.ASCII.GetBytes("fmt "));
            // Number of bytes in the format chunk.
            _writer.Write(16);
            // Type of format (1 = PCM)
            _writer.Write((short)1);
            // Number of channels
            _writer.Write((short)_channels);
            // Sample rate in Hertz
            _writer.Write(_sampleRate);
            // Number of bytes per second = Sample Rate * BytesPerSample * Channels
            _writer.Write(_sampleRate * BytesPerSample * _channels);
            // Number of bytes for one sample including all channels
            _writer.Write((short)(BytesPerSample * _channels));
            // Number of bits per sample
            _writer.Write((short) (BytesPerSample * 8));
            
            // Data chunk marker
            _writer.Write(Encoding.ASCII.GetBytes("data"));
            // Size of the data section. Size of the overall file - 44 bytes (for the header).
            _writer.Write((int)(_writer.BaseStream.Length - HeaderSize));
        }

        public void Close()
        {
            _stream?.Dispose();
            _writer?.Dispose();
        }

        public void Dispose()
        {
            Close();
        }
    }
}