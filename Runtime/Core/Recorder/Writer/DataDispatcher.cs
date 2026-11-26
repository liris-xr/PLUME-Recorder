using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine.Profiling;

namespace PLUME.Core.Recorder.Writer
{
    public class DataDispatcher
    {
        // Read by the dispatcher thread.
        private volatile bool _running;
        private Thread _dispatcherThread;

        private IDataWriter[] _outputs;
        // Held around every write batch, flush and close: outputs are not thread-safe and flush runs on the main thread.
        private readonly object _outputsLock = new();
        private NetworkStream _networkStream;

        internal void Start(Record record)
        {
            var fileDataWriter = new FileDataWriter(record);
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

            try
            {
                while (_running)
                {
                    // Data that fails to be written is lost, but the thread must survive to write the rest.
                    try
                    {
                        DispatchAvailableData(record, tmpTimelessDataChunks, tmpDataChunksTimestamped);
                    }
                    catch (Exception e)
                    {
                        ErrorReporter.Report($"{nameof(DataDispatcher)}.{nameof(DispatchLoop)}",
                            "Failed to write recorded data. That data is lost, recording continues.", e);
                    }

                    if (_running)
                        Thread.Sleep(1);
                }

                DispatchRemainingData(record, tmpTimelessDataChunks, tmpDataChunksTimestamped);
            }
            catch (Exception e)
            {
                ErrorReporter.Report($"{nameof(DataDispatcher)}.{nameof(DispatchRemainingData)}",
                    "Failed to write the end of the record. The record file may be truncated.", e);
            }
            finally
            {
                tmpTimelessDataChunks.Dispose();
                tmpDataChunksTimestamped.Dispose();
                Profiler.EndThreadProfiling();
            }
        }

        private void DispatchAvailableData(Record record, DataChunks tmpTimelessDataChunks,
            DataChunksTimestamped tmpDataChunksTimestamped)
        {
            var timestamp = record.Time;
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
        }

        private void DispatchRemainingData(Record record, DataChunks tmpTimelessDataChunks,
            DataChunksTimestamped tmpDataChunksTimestamped)
        {
            tmpTimelessDataChunks.Clear();
            tmpDataChunksTimestamped.Clear();

            if (record.TryRemoveAllTimelessDataChunks(tmpTimelessDataChunks))
            {
                DispatchTimelessDataChunks(tmpTimelessDataChunks);
            }

            if (record.TryRemoveAllTimestampedDataChunks(tmpDataChunksTimestamped))
            {
                DispatchTimestampedDataChunks(tmpDataChunksTimestamped);
            }
        }

        private void DispatchTimelessDataChunks(DataChunks dataChunks)
        {
            if (_outputs == null)
                return;

            lock (_outputsLock)
            {
                foreach (var output in _outputs)
                {
                    output.WriteTimelessData(dataChunks);
                }
            }
        }

        private void DispatchTimestampedDataChunks(DataChunksTimestamped dataChunks)
        {
            if (_outputs == null)
                return;
            
            // TODO: Dispatcher shouldn't wait for the writers to finish. Instead they should copy the data and handle it themselves. 
            lock (_outputsLock)
            {
                foreach (var output in _outputs)
                {
                    output.WriteTimestampedData(dataChunks);
                }
            }
        }

        internal async Task StopAsync()
        {
            _running = false;

            if (_dispatcherThread != null)
                await Task.Run(_dispatcherThread.Join);

            CloseOutputs();
        }

        internal void Stop()
        {
            _running = false;
            _dispatcherThread?.Join();
            CloseOutputs();
        }

        private void CloseOutputs()
        {
            _dispatcherThread = null;

            lock (_outputsLock)
            {
                if (_outputs != null)
                {
                    foreach (var output in _outputs)
                    {
                        // Each output must get the chance to close its file, even if another one fails.
                        try
                        {
                            output.Close();
                        }
                        catch (Exception e)
                        {
                            ErrorReporter.Report($"{output.GetType().Name}.{nameof(IDataWriter.Close)}",
                                "Failed to close a record output. The record file may be truncated.", e);
                        }
                    }
                }

                _outputs = null;
            }
        }

        public void OnApplicationPaused()
        {
            if (!_running)
                return;

            Logger.Log("Application paused detected. Flushing data.");

            // Synchronous on purpose: the application may be suspended as soon as this callback returns.
            lock (_outputsLock)
            {
                if (_outputs == null)
                    return;

                foreach (var output in _outputs)
                {
                    // Called from Unity's pause callback, which must never throw.
                    try
                    {
                        output.Flush();
                    }
                    catch (Exception e)
                    {
                        ErrorReporter.Report($"{output.GetType().Name}.{nameof(IDataWriter.Flush)}",
                            "Failed to flush a record output on application pause. Data may be lost if the application is killed.", e);
                    }
                }
            }
        }
    }
}