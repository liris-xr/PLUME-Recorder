using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PLUME.Core.Recorder.Module;
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
        public IReadOnlyClock Clock => _clock;

        private readonly Clock _clock;

        private IRecorderModule[] _recorderModules;
        private Dictionary<Type, IRecorderModule> _recorderModulesByType;
        
        private PlumeRecorder(Clock clock)
        {
            _clock = clock;
        }

        ~PlumeRecorder()
        {
            if (IsRecording)
                Stop();

            foreach (var module in _recorderModules)
                module.Destroy();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Instantiate()
        {
            var recorderModuleTypes = RecorderModuleManager.GetRecorderModulesTypesFromAllAssemblies();
            var recorderModules = RecorderModuleManager.InstantiateRecorderModulesFromTypes(recorderModuleTypes);
            _instance = Instantiate(new Clock(), recorderModules);
        }

        internal static PlumeRecorder Instantiate(Clock clock, IRecorderModule[] recorderModules)
        {
            var instance = new PlumeRecorder(clock)
            {
                _recorderModules = recorderModules,
                _recorderModulesByType = recorderModules.ToDictionary(module => module.GetType())
            };

            foreach (var recorderModule in recorderModules)
            {
                recorderModule.Create(instance);
            }

            return instance;
        }

        public void Start()
        {
            if (IsRecording)
                throw new InvalidOperationException("Recorder is already recording");

            _clock.Reset();
            foreach (var module in _recorderModules)
                module.Start();

            _clock.Start();
            IsRecording = true;
        }

        public void Stop()
        {
            if (!IsRecording)
                throw new InvalidOperationException("Recorder is not recording");

            _clock.Stop();

            foreach (var module in _recorderModules)
                module.Stop();

            IsRecording = false;
        }

        internal void EnsureIsRecording()
        {
            if (!IsRecording)
                throw new InvalidOperationException("Recorder is not recording");
        }

        public ReadOnlyCollection<IRecorderModule> GetRecorderModules()
        {
            return Array.AsReadOnly(_recorderModules);
        }

        public bool TryGetRecorderModule<T>(out T module) where T : IRecorderModule
        {
            if (_recorderModulesByType.TryGetValue(typeof(T), out var m))
            {
                module = (T)m;
                return true;
            }

            module = default;
            return false;
        }
        
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