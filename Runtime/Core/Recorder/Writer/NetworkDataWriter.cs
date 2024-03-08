using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using UnityEngine;

namespace PLUME.Core.Recorder.Writer
{
    public class NetworkDataWriter : IDataWriter
    {
        private readonly Stream _stream;

        public NetworkDataWriter(RecordMetadata recordMetadata)
        {
            // Create a tcp server
            var server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8000);
            server.Start();
            
            var stream = server.AcceptTcpClient().GetStream();
            
            while(!stream.CanWrite)
            {
                Debug.Log("Waiting for client to connect...");
                Thread.Sleep(100);
            }
            
            _stream = LZ4Stream.Encode(stream, LZ4Level.L00_FAST);
        }

        public void WriteTimelessData(DataChunks dataChunks)
        {
        }

        public void WriteTimestampedData(DataChunksTimestamped dataChunks)
        {
            var data = dataChunks.GetDataSpan();
            _stream.Write(data);
            _stream.Flush();
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