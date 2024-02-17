using System;
using Cysharp.Threading.Tasks;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder.Data;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Time;
using PLUME.Core.Scripts;
using PLUME.Core.Utils;
using PLUME.Sample.Common;
using Unity.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace PLUME.Core.Recorder
{
    /// <summary>
    /// The main class of the PLUME recorder. It is responsible for managing the recording process (start/stop) and the recorder modules.
    /// It is a singleton and should be accessed through the <see cref="Instance"/> property. The instance is created automatically
    /// after the assemblies are loaded by the application.
    /// </summary>
    public sealed class PlumeRecorder : IDisposable
    {
        public static PlumeRecorder Instance { get; private set; }

        public RecorderStatus CurrentStatus { get; private set; } = RecorderStatus.Stopped;

        public bool IsRecording => CurrentStatus == RecorderStatus.Recording;
        public bool IsStopping => CurrentStatus == RecorderStatus.Stopping;
        public bool IsStopped => CurrentStatus == RecorderStatus.Stopped;

        public readonly RecorderContext Context;
        private Record _record;

        private readonly DataDispatcher _dataDispatcher;

        private GameObject _applicationOnPauseListener;

        private bool _wantsToQuit;

        private PlumeRecorder(DataDispatcher dataDispatcher, RecorderContext ctx)
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
        internal static void Instantiate()
        {
            var typeUrlRegistry = new SampleTypeUrlRegistry(Allocator.Persistent);
            var objSafeRefProvider = new ObjectSafeRefProvider();
            var recorderModules = RecorderModuleManager.InstantiateRecorderModulesFromAllAssemblies();
            var dataDispatcher = new DataDispatcher();
            var recorderContext =
                new RecorderContext(Array.AsReadOnly(recorderModules), objSafeRefProvider, typeUrlRegistry);

            Instance = new PlumeRecorder(dataDispatcher, recorderContext);

            foreach (var recorderModule in recorderModules)
            {
                recorderModule.Create(recorderContext);
            }

            PlayerLoopUtils.InjectUpdateInCurrentLoop(typeof(RecorderFixedUpdate), Instance.FixedUpdate,
                typeof(FixedUpdate));
            PlayerLoopUtils.InjectUpdateInCurrentLoop(typeof(RecorderEarlyUpdate), Instance.EarlyUpdate,
                typeof(EarlyUpdate));
            PlayerLoopUtils.InjectUpdateInCurrentLoop(typeof(RecorderPreUpdate), Instance.PreUpdate,
                typeof(PreUpdate));
            PlayerLoopUtils.InjectUpdateInCurrentLoop(typeof(RecorderUpdate), Instance.Update, typeof(Update));
            PlayerLoopUtils.InjectUpdateInCurrentLoop(typeof(RecorderPreLateUpdate), Instance.PreLateUpdate,
                typeof(PreLateUpdate));
            PlayerLoopUtils.InjectUpdateInCurrentLoop(typeof(RecorderPostLateUpdate), Instance.PostLateUpdate,
                typeof(PostLateUpdate));

            ApplicationPauseDetector.Paused += () => Instance.OnApplicationPaused();

            Application.wantsToQuit += Instance.OnApplicationWantsToQuit;

            Application.quitting += () =>
            {
                Instance.OnApplicationQuitting();
                Instance.Dispose();
                typeUrlRegistry.Dispose();
            };
        }

        private void FixedUpdate()
        {
            if (!IsRecording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Context.Modules.Count; i++)
            {
                Context.Modules[i].FixedUpdate(_record, Context);
            }
        }

        private void PreUpdate()
        {
            if (!IsRecording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Context.Modules.Count; i++)
            {
                Context.Modules[i].PreUpdate(_record, Context);
            }
        }

        private void EarlyUpdate()
        {
            if (!IsRecording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Context.Modules.Count; i++)
            {
                Context.Modules[i].EarlyUpdate(_record, Context);
            }
        }

        private void Update()
        {
            if (!IsRecording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Context.Modules.Count; i++)
            {
                Context.Modules[i].Update(_record, Context);
            }
        }

        private void PreLateUpdate()
        {
            if (!IsRecording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Context.Modules.Count; i++)
            {
                Context.Modules[i].PreLateUpdate(_record, Context);
            }
        }

        private void PostLateUpdate()
        {
            if (!IsRecording)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Context.Modules.Count; i++)
            {
                Context.Modules[i].PostLateUpdate(_record, Context);
            }
        }

        /// <summary>
        /// Starts the recording process. If the recorder is already recording, throw a <see cref="InvalidOperationException"/> exception.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// TODO: take a record identifier as param
        internal void StartRecordingInternal(RecordIdentifier recordIdentifier)
        {
            if (IsStopping)
                throw new InvalidOperationException(
                    "Recorder is stopping. You cannot start it again until it is stopped.");

            if (IsRecording)
                throw new InvalidOperationException("Recorder is already recording.");

            ApplicationPauseDetector.EnsureExists();

            var recordClock = new Clock();
            var data = new RecordDataBuffer(Allocator.Persistent);
            _record = new Record(recordClock, data, recordIdentifier);

            CurrentStatus = RecorderStatus.Recording;

            _dataDispatcher.Start(_record);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Context.Modules.Count; i++)
            {
                Context.Modules[i].StartRecording(_record, Context);
            }

            recordClock.Start();

            Logger.Log("Recorder started.");
        }

        /// <summary>
        /// Stops the recording process. If the recorder is not recording, throw a <see cref="InvalidOperationException"/> exception.
        /// This method calls the <see cref="IRecorderModule.StopRecording"/> method on all the recorder modules and stops the <see cref="FrameRecorder"/>.
        /// This method also stops the clock without resetting it.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if called when the recorder is not recording.</exception>
        internal async UniTask StopRecordingInternal()
        {
            if (IsStopping)
                throw new InvalidOperationException("Recorder is already stopping.");

            if (!IsRecording)
                throw new InvalidOperationException("Recorder is not recording.");

            Logger.Log("Stopping recorder...");

            _record.InternalClock.Stop();
            CurrentStatus = RecorderStatus.Stopping;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Context.Modules.Count; i++)
            {
                await Context.Modules[i].StopRecording(_record, Context);
            }

            await _dataDispatcher.Stop();

            CurrentStatus = RecorderStatus.Stopped;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Context.Modules.Count; i++)
            {
                Context.Modules[i].Reset(Context);
            }

            ApplicationPauseDetector.Destroy();
            _record.Dispose();
            _record = null;

            Logger.Log("Recorder stopped.");
        }

        internal void ForceStopRecordingInternal()
        {
            if (IsStopped)
                throw new InvalidOperationException("Recorder is already stopped.");

            Logger.Log("Force stopping recorder...");

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Context.Modules.Count; i++)
            {
                Context.Modules[i].ForceStopRecording(_record, Context);
            }

            _dataDispatcher.ForceStop();
            CurrentStatus = RecorderStatus.Stopped;
            
            _record.Dispose();
            _record = null;

            Logger.Log("Recorder force stopped.");
        }

        private void RecordMarkerInternal(string label)
        {
            EnsureIsRecording();
            var marker = new Marker {Label = label};
            _record.DataBuffer.AddTimestampedSample(marker, _record.Clock.ElapsedNanoseconds);
        }

        private void StartRecordingObjectInternal<T>(ObjectSafeRef<T> objectSafeRef, bool markCreated)
            where T : UnityEngine.Object
        {
            EnsureIsRecording();
            
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Context.Modules.Count; i++)
            {
                var module = Context.Modules[i];
                if (module is not IObjectRecorderModule objectRecorderModule)
                    continue;

                if (objectRecorderModule.SupportedObjectType != objectSafeRef.ObjectType)
                    continue;

                if (objectRecorderModule.IsRecordingObject(objectSafeRef))
                    continue;

                objectRecorderModule.StartRecordingObject(objectSafeRef, markCreated);
            }
        }

        private void StopRecordingObjectInternal<T>(ObjectSafeRef<T> objectSafeRef, bool markDestroyed)
            where T : UnityEngine.Object
        {
            EnsureIsRecording();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Context.Modules.Count; i++)
            {
                var module = Context.Modules[i];

                if (module is not IObjectRecorderModule objectRecorderModule)
                    continue;

                if (objectRecorderModule.SupportedObjectType != objectSafeRef.ObjectType)
                    continue;

                if (!objectRecorderModule.IsRecordingObject(objectSafeRef))
                    continue;

                objectRecorderModule.StopRecordingObject(objectSafeRef, markDestroyed);
            }
        }

        private void OnApplicationPaused()
        {
            _dataDispatcher?.OnApplicationPaused();
        }

        private bool OnApplicationWantsToQuit()
        {
            if (Application.isEditor)
                return true;

            if (IsStopped)
                return true;

            if (IsRecording)
            {
                var stopTask = StopRecordingInternal();
                if (!stopTask.Status.IsCompleted())
                {
                    Logger.Log(
                        "Waiting for the recorder modules to stop before quitting the application. The application will stop automatically when finished.");
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
                ForceStopRecordingInternal();
        }

        /// <summary>
        /// Ensures that the recorder is recording. If it is not, an <see cref="InvalidOperationException"/> is thrown.
        /// Only called by internal methods.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the recorder is not recording.</exception>
        private void EnsureIsRecording()
        {
            if (!IsRecording)
                throw new InvalidOperationException("Recorder is not recording.");
        }

        private static void CheckInstantiated()
        {
            if (Instance == null)
                throw new InvalidOperationException("PLUME recorder instance is not created yet.");
        }

        public void Dispose()
        {
            foreach (var module in Context.Modules)
                module.Destroy(Context);
        }

        public static void StartRecording(RecordIdentifier recordIdentifier)
        {
            CheckInstantiated();
            Instance.StartRecordingInternal(recordIdentifier);
        }

        public static async UniTask StopRecording()
        {
            CheckInstantiated();
            await Instance.StopRecordingInternal();
        }
        
        public static void ForceStopRecording()
        {
            Instance.ForceStopRecordingInternal();
        }

        public static void RecordMarker(string label)
        {
            CheckInstantiated();
            Instance.RecordMarkerInternal(label);
        }

        public static void StartRecordingObject<T>(ObjectSafeRef<T> objectSafeRef, bool markCreated = true)
            where T : UnityEngine.Object
        {
            CheckInstantiated();
            Instance.StartRecordingObjectInternal(objectSafeRef, markCreated);
        }

        public static void StopRecordingObject<T>(ObjectSafeRef<T> objectSafeRef, bool markDestroyed = true)
            where T : UnityEngine.Object
        {
            CheckInstantiated();
            Instance.StopRecordingObjectInternal(objectSafeRef, markDestroyed);
        }

        public static void StartRecordingObject<T>(T obj, bool markCreated = true) where T : UnityEngine.Object
        {
            CheckInstantiated();
            var objectSafeRef = Instance.Context.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(obj);
            Instance.StartRecordingObjectInternal(objectSafeRef, markCreated);
        }

        public static void StopRecordingObject<T>(T obj, bool markDestroyed = true) where T : UnityEngine.Object
        {
            CheckInstantiated();
            var objectSafeRef = Instance.Context.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(obj);
            Instance.StopRecordingObjectInternal(objectSafeRef, markDestroyed);
        }
    }
}