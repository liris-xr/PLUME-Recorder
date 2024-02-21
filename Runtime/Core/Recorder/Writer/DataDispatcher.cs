using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine.Profiling;

namespace PLUME.Core.Recorder.Writer
{
    public class DataDispatcher
    {
        private bool _running;
        private Thread _dispatcherThread;

        private IDataWriter[] _outputs;
        private NetworkStream _networkStream;

        internal void Start(Record record)
        {
            var recordIdentifier = record.Identifier;
            
            var fileDataWriter = new FileDataWriter(recordIdentifier);
            _outputs = new IDataWriter[] { fileDataWriter };
            
            // var networkDataWriter = new NetworkDataWriter(recordIdentifier);
            // _outputs = new IDataWriter[] { networkDataWriter };

            _dispatcherThread = new Thread(() => DispatchLoop(record))
            {
                Name = "DataDispatcher.DispatchThread",
                IsBackground = false
            };
            _running = true;
            _dispatcherThread.Start();
        }

        private void DispatchLoop(Record record)
        {
            Profiler.BeginThreadProfiling("PLUME", "DataDispatcher");

            DataChunks tmpTimelessDataChunks = new(Allocator.Persistent);
            DataChunksTimestamped tmpDataChunksTimestamped = new(Allocator.Persistent);

            while (_running)
            {
                var timestamp = record.Clock.ElapsedNanoseconds;
                var timeBarrier = timestamp - 1_000_000;
                
                tmpTimelessDataChunks.Clear();
                tmpDataChunksTimestamped.Clear();

                if (record.TryRemoveAllTimelessDataChunks(tmpTimelessDataChunks))
                {
                    DispatchTimelessDataChunks(tmpTimelessDataChunks);
                }
                
                if (record.TryRemoveAllTimestampedDataChunksBeforeTimestamp(timeBarrier, tmpDataChunksTimestamped,
                        true))
                {
                    DispatchTimestampedDataChunks(tmpDataChunksTimestamped);
                }

                if (_running)
                    Thread.Sleep(1);
            }

            tmpTimelessDataChunks.Clear();
            tmpDataChunksTimestamped.Clear();
            
            // Dispatch any remaining data
            if (record.TryRemoveAllTimelessDataChunks(tmpTimelessDataChunks))
            {
                DispatchTimelessDataChunks(tmpTimelessDataChunks);
            }

            if (record.TryRemoveAllTimestampedDataChunks(tmpDataChunksTimestamped))
            {
                DispatchTimestampedDataChunks(tmpDataChunksTimestamped);
            }

            tmpTimelessDataChunks.Dispose();
            tmpDataChunksTimestamped.Dispose();

            Profiler.EndThreadProfiling();
        }

        private void DispatchTimelessDataChunks(DataChunks dataChunks)
        {
            if (_outputs == null)
                return;

            foreach (var output in _outputs)
            {
                output.WriteTimelessData(dataChunks);
            }
        }

        private void DispatchTimestampedDataChunks(DataChunksTimestamped dataChunks)
        {
            if (_outputs == null)
                return;
            
            // TODO: Dispatcher shouldn't wait for the writers to finish. Instead they should copy the data and handle it themselves. 
            foreach (var output in _outputs)
            {
                output.WriteTimestampedData(dataChunks);
            }
        }

        internal async UniTask Stop()
        {
            _running = false;
            await UniTask.WaitUntil(() => !_dispatcherThread.IsAlive);
            _dispatcherThread = null;

            if (_outputs != null)
            {
                foreach (var output in _outputs)
                {
                    output.Close();
                }
            }

            _outputs = null;
        }

        internal void ForceStop()
        {
            _running = false;
            _dispatcherThread.Join();
            _dispatcherThread = null;
            
            if (_outputs != null)
            {
                foreach (var output in _outputs)
                {
                    output.Close();
                }
            }
            _outputs = null;
        }

        public void OnApplicationPaused()
        {
            if (!_running)
                return;

            Logger.Log("Application paused detected. Flushing data.");

            foreach (var output in _outputs)
            {
                output.Flush();
            }
        }
    }
}