using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.Time;
using PLUME.Core.Recorder.Writer;
using PLUME.Core.Utils;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace PLUME.Core.Recorder
{
    public class DataDispatcher
    {
        private bool _shouldUpdate;

        private readonly IReadOnlyClock _clock;
        private readonly IRecorderData _recorderData;
        private IDataWriter[] _outputs;

        private DataDispatcher(IReadOnlyClock clock, IRecorderData recorderData)
        {
            _clock = clock;
            _recorderData = recorderData;
        }

        internal static DataDispatcher Instantiate(IReadOnlyClock clock, IRecorderData recorderData,
            bool injectUpdateInCurrentLoop)
        {
            var dataDispatcher = new DataDispatcher(clock, recorderData);

            if (injectUpdateInCurrentLoop)
            {
                dataDispatcher.InjectUpdateInCurrentLoop();
            }

            return dataDispatcher;
        }

        internal void InjectUpdateInCurrentLoop()
        {
            PlayerLoopUtils.InjectUpdateInCurrentLoop(typeof(FrameRecorder), Update, typeof(PostLateUpdate));
        }

        internal void Start(IDataWriter[] outputs)
        {
            Debug.Log($"Started data dispatcher with {outputs.Length} outputs.");
            _shouldUpdate = true;
            _outputs = outputs;
        }

        internal void Stop()
        {
            _shouldUpdate = false;
            DispatchAllData();
            _outputs = null;
        }

        private void Update()
        {
            if (!_shouldUpdate)
                return;

            var timestamp = _clock.ElapsedNanoseconds;
            var timeBarrier = timestamp - 1_000_000;
            
            DispatchDataBeforeTimestamp(timeBarrier);
        }

        private void DispatchAllData()
        {
            var timelessData = new NativeList<byte>(Allocator.Persistent);
            var timelessDataLengths = new NativeList<int>(Allocator.Persistent);

            var hasTimelessData = _recorderData.TryPopTimelessData(timelessData, timelessDataLengths);

            var timestampedData = new NativeList<byte>(Allocator.Persistent);
            var timestampedLengths = new NativeList<int>(Allocator.Persistent);
            var timestamps = new NativeList<long>(Allocator.Persistent);

            var hasTimestampedData = _recorderData.TryPopAllTimestampedData(timestampedData, timestampedLengths, timestamps);

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

            var hasTimelessData = _recorderData.TryPopTimelessData(timelessData, timelessDataLengths);

            var timestampedData = new NativeList<byte>(Allocator.Persistent);
            var timestampedLengths = new NativeList<int>(Allocator.Persistent);
            var timestamps = new NativeList<long>(Allocator.Persistent);

            var hasTimestampedData = _recorderData.TryPopTimestampedDataBeforeTimestamp(timeBarrier, timestampedData,
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
    }
}