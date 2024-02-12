using System;
using System.Collections.ObjectModel;
using System.Linq;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Recorder.Time;
using UnityEngine;

namespace PLUME.Core.Recorder
{
    /// <summary>
    /// The main class of the PLUME recorder. It is responsible for managing the recording process (start/stop) and the recorder modules.
    /// It is a singleton and should be accessed through the <see cref="Instance"/> property. The instance is created automatically
    /// after the assemblies are loaded by the application.
    /// </summary>
    public sealed class PlumeRecorder
    {
        private static PlumeRecorder _instance;

        public bool IsRecording { get; private set; }

        public readonly ObjectSafeRefProvider ObjectSafeRefProvider;

        /// <summary>
        /// Clock used by the recorder to timestamp the samples.
        /// The clock is automatically started and stopped when the recorder starts and stops.
        /// </summary>
        private readonly Clock _clock;

        private readonly IRecorderModule[] _recorderModules;

        /// <summary>
        /// 
        /// </summary>
        private readonly FrameRecorder _frameRecorder;

        private PlumeRecorder(IRecorderModule[] recorderModules)
        {
            _clock = new Clock();
            ObjectSafeRefProvider = new ObjectSafeRefProvider();
            _recorderModules = recorderModules;
            var frameDataRecorderModules = recorderModules.OfType<IFrameDataRecorderModule>().ToArray();
            var asyncFrameDataRecorderModules = recorderModules.OfType<IFrameDataRecorderModuleAsync>().ToArray();
            var frameSamplePacker = new FrameSamplePacker();
            _frameRecorder = new FrameRecorder(_clock, frameDataRecorderModules, asyncFrameDataRecorderModules,
                frameSamplePacker);
        }

        ~PlumeRecorder()
        {
            if (IsRecording)
                Stop();

            foreach (var module in _recorderModules)
                module.Destroy();
        }

        /// <summary>
        /// Instantiates the recorder instance after the assemblies are loaded by the application.
        /// This method is called automatically by the application.
        /// Recorder modules are found by scanning all the assemblies and are instantiated using their default constructor.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Instantiate()
        {
            var recorderModuleTypes = RecorderModuleManager.GetRecorderModulesTypesFromAllAssemblies();
            var recorderModules = RecorderModuleManager.InstantiateRecorderModulesFromTypes(recorderModuleTypes);
            _instance = Instantiate(recorderModules, true);
        }

        /// <summary>
        /// Instantiates a new instance of the recorder with the specified clock and recorder modules.
        /// For internal use only. Use the <see cref="Instance"/> property to get the instance of the recorder.
        /// </summary>
        /// <param name="recorderModules">The recorder modules attached to the recorder.</param>
        /// <param name="injectUpdateInCurrentLoop">If true, injects the <see cref="FrameRecorder"/> update method in the player loop.</param>
        /// <returns>The new instance of the recorder.</returns>
        internal static PlumeRecorder Instantiate(IRecorderModule[] recorderModules, bool injectUpdateInCurrentLoop)
        {
            var instance = new PlumeRecorder(recorderModules);

            if (injectUpdateInCurrentLoop)
                instance._frameRecorder.InjectUpdateInCurrentLoop();

            return instance;
        }

        /// <summary>
        /// Starts the recording process. If the recorder is already recording, throw a <see cref="InvalidOperationException"/> exception.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start()
        {
            if (IsRecording)
                throw new InvalidOperationException("Recorder is already recording");

            _clock.Reset();
            IsRecording = true;

            _frameRecorder.Start();

            foreach (var module in _recorderModules)
                module.Start();

            _clock.Start();
        }

        /// <summary>
        /// Stops the recording process. If the recorder is not recording, throw a <see cref="InvalidOperationException"/> exception.
        /// This method calls the <see cref="IRecorderModule.Stop"/> method on all the recorder modules and stops the <see cref="FrameRecorder"/>.
        /// This method also stops the clock without resetting it.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if called when the recorder is not recording.</exception>
        public void Stop()
        {
            if (!IsRecording)
                throw new InvalidOperationException("Recorder is not recording");

            IsRecording = false;
            _clock.Stop();

            _frameRecorder.Stop();

            foreach (var module in _recorderModules)
                module.Stop();
        }

        /// <summary>
        /// Ensures that the recorder is recording. If it is not, an <see cref="InvalidOperationException"/> is thrown.
        /// Only called by internal methods.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the recorder is not recording.</exception>
        internal void EnsureIsRecording()
        {
            if (!IsRecording)
                throw new InvalidOperationException("Recorder is not recording");
        }

        /// <summary>
        /// Tries to get a module of the specified type. If the module is found, it is returned through the out parameter and the method returns true.
        /// Note that this is a linear search. If you need to get a module multiple times, consider caching the result.
        /// </summary>
        /// <param name="module">The module of the specified type, if found.</param>
        /// <typeparam name="T">The type of the module to get.</typeparam>
        /// <returns>True if the module is found, false otherwise.</returns>
        public bool TryGetModule<T>(out T module) where T : IRecorderModule
        {
            var recorderModule = _recorderModules.OfType<T>();
            module = recorderModule.FirstOrDefault();
            return module != null;
        }

        /// <summary>
        /// List of all the modules attached to the recorder.
        /// </summary>
        public ReadOnlyCollection<IRecorderModule> Modules => Array.AsReadOnly(_recorderModules);

        public IReadOnlyClock Clock => _clock;

        public static PlumeRecorder Instance
        {
            get
            {
                if (_instance == null)
                    Instantiate();

                return _instance;
            }
        }
    }
}