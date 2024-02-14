using System;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Writer;
using PLUME.Core.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace PLUME.Core.Recorder
{
    public class DataDispatcher : IDisposable
    {
        private bool _shouldUpdate;
    
        private RecordContext _recordContext;
        private IDataWriter[] _outputs;
        private FileDataWriter _fileDataWriter;

        private DataDispatcher()
        {
        }

        internal static DataDispatcher Instantiate(bool injectUpdateInCurrentLoop)
        {
            var dataDispatcher = new DataDispatcher();

            if (injectUpdateInCurrentLoop)
            {
                dataDispatcher.InjectUpdateInCurrentLoop();
            }

            return dataDispatcher;
        }

        internal void InjectUpdateInCurrentLoop()
        {
            PlayerLoopUtils.InjectUpdateInCurrentLoop(typeof(FrameRecorderModule), Update, typeof(PostLateUpdate));
        }

        internal void Start(RecordContext recordContext)
        {
            _recordContext = recordContext;
            
            var recordIdentifier = recordContext.Identifier;
            // TODO: format string
            _fileDataWriter = new FileDataWriter(Application.persistentDataPath,
                recordIdentifier.Identifier + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            
            _shouldUpdate = true;
            _outputs = new IDataWriter[] { _fileDataWriter };
        }

        internal void Stop()
        {
            _shouldUpdate = false;
            DispatchAllData();
            _outputs = null;
            
            _recordContext = null;
        }

        private void Update()
        {
            if (!_shouldUpdate)
                return;

            var timestamp = _recordContext.Clock.ElapsedNanoseconds;
            var timeBarrier = timestamp - 1_000_000;
            
            DispatchDataBeforeTimestamp(timeBarrier);
        }

        private void DispatchAllData()
        {
            var timelessData = new NativeList<byte>(Allocator.Persistent);
            var timelessDataLengths = new NativeList<int>(Allocator.Persistent);

            var hasTimelessData = _recordContext.Data.TryPopTimelessData(timelessData, timelessDataLengths);

            var timestampedData = new NativeList<byte>(Allocator.Persistent);
            var timestampedLengths = new NativeList<int>(Allocator.Persistent);
            var timestamps = new NativeList<long>(Allocator.Persistent);

            var hasTimestampedData = _recordContext.Data.TryPopAllTimestampedData(timestampedData, timestampedLengths, timestamps);

            if (hasTimelessData)
            {
                foreach (var output in _outputs)
                {
                    output.WriteTimelessData(timelessData.AsArray(), timelessDataLengths.AsArray());
                }
            }

            if (hasTimestampedData)
            {
                foreach (var output in _outputs)
                {
                    output.WriteTimestampedData(timestampedData.AsArray(), timestampedLengths.AsArray(),
                        timestamps.AsArray());
                }
            }

            timelessData.Dispose();
            timelessDataLengths.Dispose();

            timestampedData.Dispose();
            timestampedLengths.Dispose();
            timestamps.Dispose();
        }

        private void DispatchDataBeforeTimestamp(long timeBarrier)
        {
            var timelessData = new NativeList<byte>(Allocator.Persistent);
            var timelessDataLengths = new NativeList<int>(Allocator.Persistent);

            var hasTimelessData = _recordContext.Data.TryPopTimelessData(timelessData, timelessDataLengths);

            var timestampedData = new NativeList<byte>(Allocator.Persistent);
            var timestampedLengths = new NativeList<int>(Allocator.Persistent);
            var timestamps = new NativeList<long>(Allocator.Persistent);

            var hasTimestampedData = _recordContext.Data.TryPopTimestampedDataBeforeTimestamp(timeBarrier, timestampedData,
                timestampedLengths, timestamps, true);

            if (hasTimelessData)
            {
                foreach (var output in _outputs)
                {
                    output.WriteTimelessData(timelessData.AsArray(), timelessDataLengths.AsArray());
                }
            }

            if (hasTimestampedData)
            {
                foreach (var output in _outputs)
                {
                    output.WriteTimestampedData(timestampedData.AsArray(), timestampedLengths.AsArray(),
                        timestamps.AsArray());
                }
            }

            timelessData.Dispose();
            timelessDataLengths.Dispose();

            timestampedData.Dispose();
            timestampedLengths.Dispose();
            timestamps.Dispose();
        }

        public void Dispose()
        {
            _fileDataWriter?.Dispose();
        }
    }
}