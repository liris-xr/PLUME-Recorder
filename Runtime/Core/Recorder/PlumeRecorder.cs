using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Time;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace PLUME.Core.Recorder
{
    public sealed class PlumeRecorder
    {
        private static PlumeRecorder _instance;

        private bool _isRecording;

        private readonly Clock _clock;

        private IRecorderModule[] _recorderModules;
        private Dictionary<Type, IRecorderModule> _recorderModulesByType;

        public static PlumeRecorder Instance
        {
            get
            {
                if (_instance == null)
                    Instantiate();

                return _instance;
            }
        }
        
        private PlumeRecorder(Clock clock)
        {
            _clock = clock;
        }

        ~PlumeRecorder()
        {
            if (_isRecording)
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
            if (_isRecording)
                throw new InvalidOperationException("Recorder is already recording");

            _clock.Reset();
            foreach (var module in _recorderModules)
                module.Start();

            _clock.Start();
            _isRecording = true;
        }

        public void Stop()
        {
            if (!_isRecording)
                throw new InvalidOperationException("Recorder is not recording");

            _clock.Stop();

            foreach (var module in _recorderModules)
                module.Stop();

            _isRecording = false;
        }

        internal void EnsureIsRecording()
        {
            if (!_isRecording)
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

        public IReadOnlyClock GetClock()
        {
            return _clock;
        }
    }
}