using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using PLUME.Core.Recorder.Data;

namespace PLUME.Core.Recorder.Writer
{
    public class NetworkDataWriter : IDataWriter
    {
        private readonly Stream _stream;

        public NetworkDataWriter(string recordIdentifier)
        {
            var stream = new TcpClient("localhost", 12345).GetStream();
            _stream = new DeflateStream(stream, CompressionLevel.Optimal);
        }

        public void WriteTimelessData(DataChunks dataChunks)
        {
        }

        public void WriteTimestampedData(TimestampedDataChunks dataChunks)
        {
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
    }
}