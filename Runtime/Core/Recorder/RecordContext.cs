using System;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.Time;

namespace PLUME.Core.Recorder
{
    public class RecordContext
    {
        /// <summary>
        /// Clock used by the recorder to timestamp the samples.
        /// The clock is automatically started and stopped when calling <see cref="Recorder.Start"/> and <see cref="Recorder.Stop"/>.
        /// </summary>
        internal readonly Clock InternalClock;

        public IReadOnlyClock Clock => InternalClock;
        public readonly IRecordData Data;
        public readonly RecordIdentifier Identifier;

        public RecordContext(Clock clock, IRecordData data, RecordIdentifier identifier)
        {
            InternalClock = clock;
            Data = data;
            Identifier = identifier;
        }
    }
}