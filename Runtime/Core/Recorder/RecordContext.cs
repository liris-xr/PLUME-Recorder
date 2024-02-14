using System;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.Time;
using Unity.Collections;

namespace PLUME.Core.Recorder
{
    public class RecordContext : IDisposable
    {
        /// <summary>
        /// Clock used by the recorder to timestamp the samples.
        /// The clock is automatically started and stopped when calling <see cref="Recorder.Start"/> and <see cref="Recorder.Stop"/>.
        /// </summary>
        internal readonly Clock InternalClock;
        
        public IReadOnlyClock Clock => InternalClock;
        public readonly IRecordData Data;
        public readonly RecordIdentifier Identifier;

        public RecordContext(Allocator allocator, Clock clock, RecordIdentifier identifier)
        {
            InternalClock = clock;
            Data = new ConcurrentRecordData(allocator);
            Identifier = identifier;
        }

        public void Dispose()
        {
            Data.Dispose();
        }
    }
}