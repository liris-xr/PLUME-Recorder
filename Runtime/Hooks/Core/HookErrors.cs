using System;
using UnityEngine;

namespace PLUME.Core.Hooks
{
    /// <summary>
    /// Reports failures caught in hooks. The hooks assembly cannot reference the recorder (see
    /// PLUME.Recorder.Hooks.asmdef), so the recorder plugs its own reporter in when it starts.
    /// </summary>
    public static class HookErrors
    {
        internal static Action<string, string, Exception> Reporter;

        /// <param name="key">Identifies the failure site, so that a repeated failure can be reported once.</param>
        /// <param name="message">Explains the consequence of the failure for the recording.</param>
        /// <param name="exception">The caught exception.</param>
        internal static void Report(string key, string message, Exception exception)
        {
            if (Reporter != null)
            {
                Reporter(key, message, exception);
                return;
            }

            Debug.LogError(message);
            Debug.LogException(exception);
        }
    }
}
