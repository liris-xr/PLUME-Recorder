using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.Writer;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace PLUME.Core.Recorder
{
    public class DataDispatcher
    {
        private bool _running;
        private Thread _dispatcherThread;

        private IDataWriter[] _outputs;
        private FileDataWriter _fileDataWriter;

        internal void Start(Record record)
        {
            var recordIdentifier = record.Identifier;
            // TODO: format string
            _fileDataWriter = new FileDataWriter(Application.persistentDataPath,
                recordIdentifier.Identifier + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));

            _outputs = new IDataWriter[] { _fileDataWriter };

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
            TimestampedDataChunks tmpTimestampedDataChunks = new(Allocator.Persistent);

            while (_running)
            {
                var timestamp = record.Clock.ElapsedNanoseconds;
                var timeBarrier = timestamp - 1_000_000;

                if (record.TryRemoveAllTimelessDataChunks(tmpTimelessDataChunks))
                {
                    DispatchTimelessDataChunks(tmpTimelessDataChunks);
                }

                if (record.TryRemoveAllTimestampedDataChunksBeforeTimestamp(timeBarrier, tmpTimestampedDataChunks, true))
                {
                    DispatchTimestampedDataChunks(tmpTimestampedDataChunks);
                }
                
                Thread.Sleep(10);
            }

            // Dispatch any remaining data
            if (record.TryRemoveAllTimelessDataChunks(tmpTimelessDataChunks))
            {
                DispatchTimelessDataChunks(tmpTimelessDataChunks);
            }

            if (record.TryRemoveAllTimestampedDataChunks(tmpTimestampedDataChunks))
            {
                DispatchTimestampedDataChunks(tmpTimestampedDataChunks);
            }

            tmpTimelessDataChunks.Dispose();
            tmpTimestampedDataChunks.Dispose();

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

        private void DispatchTimestampedDataChunks(TimestampedDataChunks dataChunks)
        {
            if (_outputs == null)
                return;

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
            _outputs = null;
            _fileDataWriter.Dispose();
        }

        internal void ForceStop()
        {
            _running = false;
            _dispatcherThread.Join();
            _dispatcherThread = null;
            _outputs = null;
            _fileDataWriter.Dispose();
        }

        public void OnApplicationPaused()
        {
            Logger.Log("Application paused detected. Flushing data.");

            foreach (var output in _outputs)
            {
                output.Flush();
            }
        }
    }
}