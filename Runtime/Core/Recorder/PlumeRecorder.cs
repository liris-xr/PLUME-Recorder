using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Time;
using Unity.Collections;
using UnityEngine;

namespace PLUME.Core.Recorder
{
    /// <summary>
    /// The main class of the PLUME recorder. It is responsible for managing the recording process (start/stop) and the recorder modules.
    /// It is a singleton and should be accessed through the <see cref="Instance"/> property. The instance is created automatically
    /// after the assemblies are loaded by the application.
    /// </summary>
    public sealed class PlumeRecorder : IDisposable
    {
        private static PlumeRecorder _instance;

        public Status CurrentStatus { get; private set; } = Status.Stopped;

        public bool IsRecording => CurrentStatus == Status.Recording;
        public bool IsStopping => CurrentStatus == Status.Stopping;
        public bool IsStopped => CurrentStatus == Status.Stopped;

        public readonly ObjectSafeRefProvider ObjectSafeRefProvider;

        public readonly SampleTypeUrlRegistry SampleTypeUrlRegistry;

        /// <summary>
        /// Clock used by the recorder to timestamp the samples.
        /// The clock is automatically started and stopped when the recorder starts and stops.
        /// </summary>
        private readonly Clock _clock;

        private readonly IRecorderModule[] _recorderModules;

        private readonly FrameRecorder _frameRecorder;

        private CancellationTokenSource _stoppingCancellationTokenSource;

        private bool _hasScheduledQuit;

        private PlumeRecorder(Clock clock, IRecorderModule[] recorderModules,
            SampleTypeUrlRegistry sampleTypeUrlRegistry,
            ObjectSafeRefProvider objSafeRefProvider, FrameRecorder frameRecorder)
        {
            _clock = clock;
            ObjectSafeRefProvider = objSafeRefProvider;
            SampleTypeUrlRegistry = sampleTypeUrlRegistry;
            _recorderModules = recorderModules;
            _frameRecorder = frameRecorder;
        }

        ~PlumeRecorder()
        {
            Dispose();
        }

        /// <summary>
        /// Instantiates the recorder instance after the assemblies are loaded by the application.
        /// This method is called automatically by the application.
        /// Recorder modules are found by scanning all the assemblies and are instantiated using their default constructor.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Instantiate()
        {
            var clock = new Clock();
            var typeUrlRegistry = new SampleTypeUrlRegistry(Allocator.Persistent);
            var objSafeRefProvider = new ObjectSafeRefProvider();
            var recorderModules = RecorderModuleManager.InstantiateRecorderModulesFromAllAssemblies();
            var frameSamplePacker = new FrameSamplePacker();
            var frameRecorder =
                FrameRecorder.Instantiate(clock, typeUrlRegistry, recorderModules, frameSamplePacker, true);
            _instance = new PlumeRecorder(clock, recorderModules, typeUrlRegistry, objSafeRefProvider, frameRecorder);

            foreach (var recorderModule in recorderModules)
            {
                recorderModule.Create(objSafeRefProvider, typeUrlRegistry);
            }

            Application.wantsToQuit += _instance.OnApplicationWantsToQuit;
            Application.quitting += _instance.OnApplicationQuitting;
            Application.quitting += _instance.Dispose;
            Application.quitting += typeUrlRegistry.Dispose;
        }

        /// <summary>
        /// Starts the recording process. If the recorder is already recording, throw a <see cref="InvalidOperationException"/> exception.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start()
        {
            if (IsStopping)
                throw new InvalidOperationException(
                    "Recorder is stopping. You cannot start it again until it is stopped.");

            if (IsRecording)
                throw new InvalidOperationException("Recorder is already recording.");

            _clock.Reset();
            CurrentStatus = Status.Recording;

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
        public async UniTask Stop()
        {
            if (IsStopping)
                throw new InvalidOperationException("Recorder is already stopping.");

            if (!IsRecording)
                throw new InvalidOperationException("Recorder is not recording");

            _stoppingCancellationTokenSource = new CancellationTokenSource();
            CurrentStatus = Status.Stopping;
            _clock.Stop();

            try
            {
                await _frameRecorder.Stop().AttachExternalCancellation(_stoppingCancellationTokenSource.Token);

                foreach (var module in _recorderModules)
                    module.Stop();

                CurrentStatus = Status.Stopped;
            }
            catch (OperationCanceledException)
            {
                // Ignored. The recorder was force stopped.
            }

            _stoppingCancellationTokenSource.Dispose();
        }

        public void ForceStop()
        {
            if (!IsRecording)
                throw new InvalidOperationException("Recorder is not recording");

            if (IsStopping)
                _stoppingCancellationTokenSource.Cancel();

            _clock.Stop();
            _frameRecorder.ForceStop();

            foreach (var module in _recorderModules)
                module.Stop();

            CurrentStatus = Status.Stopped;
        }

        private bool OnApplicationWantsToQuit()
        {
            if (Application.isEditor)
                return true;

            if (IsStopped)
                return true;

            if (IsRecording)
            {
                var remainingTasksCount = _frameRecorder.GetRemainingTasksCount();
                Debug.Log(
                    $"Waiting for {remainingTasksCount} recording tasks to complete before quitting. The application will close automatically when finished.");
                Stop().Forget();
            }

            if (_hasScheduledQuit)
                return false;

            UniTask.WaitUntil(() => IsStopped).ContinueWith(Application.Quit).Forget();
            _hasScheduledQuit = true;
            return false;
        }

        private void OnApplicationQuitting()
        {
            if (IsRecording || IsStopping)
                ForceStop();
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

        public void Dispose()
        {
            foreach (var module in _recorderModules)
                module.Destroy();
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

        public enum Status
        {
            Recording,
            Stopping,
            Stopped
        }
    }
}