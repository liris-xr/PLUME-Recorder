using System;
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
    public sealed class Recorder : IDisposable
    {
        private static Recorder _instance;

        public Status CurrentStatus { get; private set; } = Status.Stopped;

        public bool IsRecording => CurrentStatus == Status.Recording;
        public bool IsStopping => CurrentStatus == Status.Stopping;
        public bool IsStopped => CurrentStatus == Status.Stopped;

        public readonly RecorderContext Context;
        private RecordContext _recordContext;

        private readonly DataDispatcher _dataDispatcher;

        private CancellationTokenSource _cancelStopTokenSource;

        private bool _wantsToQuit;

        private Recorder(DataDispatcher dataDispatcher, RecorderContext ctx)
        {
            Context = ctx;
            _dataDispatcher = dataDispatcher;
        }

        /// <summary>
        /// Instantiates the recorder instance after the assemblies are loaded by the application.
        /// This method is called automatically by the application.
        /// Recorder modules are found by scanning all the assemblies and are instantiated using their default constructor.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Instantiate()
        {
            var typeUrlRegistry = new SampleTypeUrlRegistry(Allocator.Persistent);
            var objSafeRefProvider = new ObjectSafeRefProvider();
            var recorderModules = RecorderModuleManager.InstantiateRecorderModulesFromAllAssemblies();
            var dataDispatcher = DataDispatcher.Instantiate(true);
            var recorderContext = new RecorderContext(Array.AsReadOnly(recorderModules), objSafeRefProvider, typeUrlRegistry);
            
            _instance = new Recorder(dataDispatcher, recorderContext);

            foreach (var recorderModule in recorderModules)
            {
                recorderModule.Create(recorderContext);
            }

            Application.wantsToQuit += _instance.OnApplicationWantsToQuit;

            Application.quitting += () =>
            {
                _instance.OnApplicationQuitting();
                _instance.Dispose();
                typeUrlRegistry.Dispose();
                dataDispatcher.Dispose();
            };
        }

        /// <summary>
        /// Starts the recording process. If the recorder is already recording, throw a <see cref="InvalidOperationException"/> exception.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// TODO: take a record identifier as param
        public void Start(RecordIdentifier recordIdentifier)
        {
            if (IsStopping)
                throw new InvalidOperationException(
                    "Recorder is stopping. You cannot start it again until it is stopped.");

            if (IsRecording)
                throw new InvalidOperationException("Recorder is already recording.");
            
            var recordClock = new Clock();
            _recordContext = new RecordContext(Allocator.Persistent, recordClock, recordIdentifier);
            
            CurrentStatus = Status.Recording;

            _dataDispatcher.Start(_recordContext);

            foreach (var module in Context.Modules)
                module.Start(_recordContext, Context);

            recordClock.Start();
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

            _cancelStopTokenSource = new CancellationTokenSource();
            CurrentStatus = Status.Stopping;
            _recordContext.InternalClock.Stop();

            try
            {
                foreach (var module in Context.Modules)
                {
                    await module.Stop(_recordContext, Context, _cancelStopTokenSource.Token);
                }

                CurrentStatus = Status.Stopped;
            }
            catch (OperationCanceledException)
            {
                // Ignored. The recorder was force stopped.
            }

            _dataDispatcher.Stop();
            _recordContext.Dispose();
            _recordContext = default;

            _cancelStopTokenSource.Dispose();
        }

        public void ForceStop()
        {
            if (!IsRecording)
                throw new InvalidOperationException("Recorder is not recording");

            if (IsStopping)
                _cancelStopTokenSource.Cancel();

            _recordContext.InternalClock.Stop();

            foreach (var module in Context.Modules)
                module.ForceStop(_recordContext, Context);

            _dataDispatcher.Stop();

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
                var stopTask = Stop();
                if (!stopTask.Status.IsCompleted())
                {
                    Debug.Log("Waiting for the recorder modules to stop before quitting the application. The application will stop automatically when finished.");
                }
            }

            if (_wantsToQuit)
                return false;

            UniTask.WaitUntil(() => IsStopped).ContinueWith(Application.Quit).Forget();
            _wantsToQuit = true;
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

        public void Dispose()
        {
            foreach (var module in Context.Modules)
                module.Destroy(Context);

            _dataDispatcher?.Dispose();
        }

        public static Recorder Instance
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