using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.Time;
using PLUME.Core.Recorder.Writer;
using UnityEngine;
using UnityEngine.Profiling;

namespace PLUME.Core.Recorder
{
    public class DataDispatcher
    {
        private bool _shouldDispatch = true;
        private Thread _dispatcherThread;

        private IDataWriter[] _outputs;
        private FileDataWriter _fileDataWriter;

        internal void Start(RecordContext recordContext)
        {
            var recordIdentifier = recordContext.Identifier;
            // TODO: format string
            _fileDataWriter = new FileDataWriter(Application.persistentDataPath,
                recordIdentifier.Identifier + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));

            _outputs = new IDataWriter[] { _fileDataWriter };

            _dispatcherThread = new Thread(() => DispatchLoop(recordContext.Data, recordContext.Clock))
            {
                Name = "DataDispatcher.DispatchThread",
                IsBackground = false
            };
            _dispatcherThread.Start();
        }

        private void DispatchLoop(IRecordData data, IReadOnlyClock clock)
        {
            Profiler.BeginThreadProfiling("PLUME", "DataDispatcher");
            
            DataChunks tmpTimelessDataChunks = new();
            DataChunks tmpTimestampedDataChunks = new();
            List<long> tmpTimestamps = new();

            while (_shouldDispatch)
            {
                var timestamp = clock.ElapsedNanoseconds;
                var timeBarrier = timestamp - 1_000_000;

                if (data.TryPopAllTimelessDataChunks(tmpTimestampedDataChunks))
                {
                    DispatchTimelessDataChunks(tmpTimelessDataChunks);
                }

                if (data.TryPopTimestampedDataChunksBefore(timeBarrier, tmpTimestampedDataChunks, tmpTimestamps, true))
                {
                    DispatchTimestampedDataChunks(tmpTimestampedDataChunks, tmpTimestamps);
                }
                
                Thread.Sleep(1);
            }

            // Dispatch any remaining data
            if (data.TryPopAllTimelessDataChunks(tmpTimestampedDataChunks))
            {
                DispatchTimelessDataChunks(tmpTimelessDataChunks);
            }

            if (data.TryPopAllTimestampedDataChunks(tmpTimestampedDataChunks, tmpTimestamps))
            {
                DispatchTimestampedDataChunks(tmpTimestampedDataChunks, tmpTimestamps);
            }
            
            Profiler.EndThreadProfiling();
        }

        private void DispatchTimelessDataChunks(DataChunks dataChunks)
        {
            if(_outputs == null)
                return;
            
            foreach (var output in _outputs)
            {
                output.WriteTimelessData(dataChunks);
            }
        }
        
        private void DispatchTimestampedDataChunks(DataChunks dataChunks, List<long> timestamps)
        {
            if(_outputs == null)
                return;
            
            foreach (var output in _outputs)
            {
                output.WriteTimestampedData(dataChunks, timestamps);
            }
        }
        
        internal async UniTask Stop()
        {
            _shouldDispatch = false;
            await UniTask.WaitUntil(() => !_dispatcherThread.IsAlive);
            _dispatcherThread = null;
            _outputs = null;
            _fileDataWriter.Dispose();
        }

        internal void ForceStop()
        {
            _shouldDispatch = false;
            _dispatcherThread.Join();
            _dispatcherThread = null;
            _outputs = null;
            _fileDataWriter.Dispose();
        }
    }
}