using System;
using Google.Protobuf.WellKnownTypes;

namespace PLUME.Core.Recorder
{
    public readonly struct RecordMetadata
    {
        public readonly string Name;
        public readonly string ExtraMetadata;

        /// <summary>
        /// Absolute UTC time at which the recording started. This *should not* be used for timestamping samples because
        /// of the low time resolution (second), use <see cref="Record.Time"/> instead for nanosecond resolution.
        /// </summary>
        public readonly DateTime StartTime;

        public RecordMetadata(string name, string extraMetadata, DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
                throw new ArgumentException("DateTime must be in UTC.", nameof(dateTime));

            Name = name;
            ExtraMetadata = extraMetadata;
            StartTime = dateTime;
        }

        public Sample.RecordMetadata ToPayload()
        {
            return new Sample.RecordMetadata
            {
                Name = Name,
                ExtraMetadata = ExtraMetadata,
                StartTime = Timestamp.FromDateTime(StartTime),
                RecorderVersion = PlumeRecorder.Version
            };
        }
    }
}