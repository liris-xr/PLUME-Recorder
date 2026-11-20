using System;
using System.Collections.Generic;

namespace PLUME.Core.Recorder.Module
{
    /// <summary>
    /// Isolates recorder modules from each other: a module that throws is disabled instead of aborting the
    /// recording, and the other modules keep recording. Thread-safe: the serialization thread reports faults too.
    /// </summary>
    internal sealed class ModuleFaults
    {
        // Value is true when the module stays disabled across recordings (it failed to initialize).
        private readonly Dictionary<IRecorderModule, bool> _disabledModules = new();

        /// <summary>
        /// Whether the module is disabled and must not be called for the rest of the recording.
        /// </summary>
        public bool IsDisabled(IRecorderModule module)
        {
            lock (_disabledModules)
            {
                return _disabledModules.ContainsKey(module);
            }
        }

        /// <summary>
        /// Whether the module failed to initialize, in which case it is never started.
        /// </summary>
        public bool IsDisabledPermanently(IRecorderModule module)
        {
            lock (_disabledModules)
            {
                return _disabledModules.TryGetValue(module, out var permanently) && permanently;
            }
        }

        /// <summary>
        /// Disables a module that threw, and logs the failure.
        /// </summary>
        /// <param name="module">The module that threw.</param>
        /// <param name="phase">The module method that threw, e.g. "StartRecording".</param>
        /// <param name="exception">The caught exception.</param>
        /// <param name="permanently">True if the module failed to initialize and must not be used again.</param>
        public void Disable(IRecorderModule module, string phase, Exception exception, bool permanently = false)
        {
            lock (_disabledModules)
            {
                _disabledModules.TryGetValue(module, out var wasPermanent);
                _disabledModules[module] = wasPermanent || permanently;
            }

            var moduleName = module.GetType().Name;
            var scope = permanently ? "for the rest of the session" : "until the end of the recording";
            ErrorReporter.Report($"{moduleName}.{phase}",
                $"{moduleName} failed during {phase} and is disabled {scope}. Other modules keep recording.",
                exception);
        }

        /// <summary>
        /// Logs a failure that only affects one object. The module stays enabled and skips that object.
        /// </summary>
        public static void ReportObjectFailure(IRecorderModule module, string phase, Exception exception)
        {
            var moduleName = module.GetType().Name;
            ErrorReporter.Report($"{moduleName}.{phase}",
                $"{moduleName} failed during {phase} for an object. That object is skipped, the module keeps recording.",
                exception);
        }

        /// <summary>
        /// Re-enables the modules disabled during the previous recording, keeping those that failed to initialize
        /// disabled.
        /// </summary>
        internal void ResetForNewRecording()
        {
            lock (_disabledModules)
            {
                var temporarilyDisabled = new List<IRecorderModule>();
                foreach (var (module, permanently) in _disabledModules)
                {
                    if (!permanently)
                        temporarilyDisabled.Add(module);
                }

                foreach (var module in temporarilyDisabled)
                    _disabledModules.Remove(module);
            }
        }
    }
}
