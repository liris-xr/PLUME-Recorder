using System.Collections.ObjectModel;
using System.Linq;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Settings;

namespace PLUME.Core.Recorder
{
    public class RecorderContext
    {
        public readonly ReadOnlyCollection<IRecorderModule> Modules;
        public readonly SafeRefProvider SafeRefProvider;
        public readonly ISettingsProvider SettingsProvider;
        
        public RecorderStatus Status { get; internal set; } = RecorderStatus.Stopped;
        public bool IsRecording => Status == RecorderStatus.Recording;
        public Record CurrentRecord { get; internal set; }

        public RecorderContext(ReadOnlyCollection<IRecorderModule> modules, SafeRefProvider safeRefProvider, ISettingsProvider settingsProvider)
        {
            Modules = modules;
            SafeRefProvider = safeRefProvider;
            SettingsProvider = settingsProvider;
        }

        /// <summary>
        /// Tries to get a module of the specified type. If the module is found, it is returned through the out parameter and the method returns true.
        /// Note that this is a linear search. If you need to get a module multiple times, consider caching the result.
        /// </summary>
        /// <param name="module">The module of the specified type, if found.</param>
        /// <typeparam name="T">The type of the module to get.</typeparam>
        /// <returns>True if the module is found, false otherwise.</returns>
        public bool TryGetRecorderModule<T>(out T module) where T : IRecorderModule
        {
            var recorderModule = Modules.OfType<T>();
            module = recorderModule.FirstOrDefault();
            return module != null;
        }
    }
}