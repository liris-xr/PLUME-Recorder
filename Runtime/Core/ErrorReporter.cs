using System;
using System.Collections.Generic;

namespace PLUME.Core
{
    /// <summary>
    /// Reports failures caught at the recorder's exception boundaries. Each distinct failure is logged once per
    /// recording, so that an error raised every frame (or for every object) does not flood the console.
    /// Thread-safe: worker threads report through it too.
    /// </summary>
    internal static class ErrorReporter
    {
        private static readonly HashSet<string> ReportedKeys = new();

        /// <summary>
        /// Logs the exception, unless a failure with the same key was already reported during this recording.
        /// </summary>
        /// <param name="key">Identifies the failure site, e.g. the module and the phase that failed.</param>
        /// <param name="message">Explains the consequence of the failure for the recording.</param>
        /// <param name="exception">The caught exception.</param>
        public static void Report(string key, string message, Exception exception)
        {
            if (!TryMarkReported(key))
                return;

            Logger.LogError(nameof(PLUME), message);
            Logger.LogException(exception);
        }

        /// <summary>
        /// Logs the warning, unless a warning with the same key was already reported during this recording.
        /// </summary>
        public static void Warn(string key, string message)
        {
            if (TryMarkReported(key))
                Logger.LogWarning(nameof(PLUME), message);
        }

        /// <summary>
        /// Lets failures be reported again, so that each recording logs its own failures.
        /// </summary>
        public static void Reset()
        {
            lock (ReportedKeys)
            {
                ReportedKeys.Clear();
            }
        }

        private static bool TryMarkReported(string key)
        {
            lock (ReportedKeys)
            {
                return ReportedKeys.Add(key);
            }
        }
    }
}
