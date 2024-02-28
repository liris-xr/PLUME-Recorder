using System;
using Cysharp.Threading.Tasks;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Writer;
using PLUME.Core.Scripts;
using PLUME.Core.Settings;
using UnityEngine;

namespace PLUME.Core.Recorder
{
    public sealed partial class PlumeRecorder
    {
        public static PlumeRecorder Instance { get; private set; }
        
        public static RecorderStatus Status => Instance._context.Status;

        public static bool IsRecording => Instance._context.Status is RecorderStatus.Recording;

        public static bool IsStopping => Instance._context.Status is RecorderStatus.Stopping;

        public static bool IsStopped => Instance._context.Status is RecorderStatus.Stopped;

        private static void CheckInstantiated()
        {
            if (Instance == null)
                throw new InvalidOperationException("PLUME recorder instance is not created yet.");
        }

        public static void StartRecording(string name, string extraMetadata = "")
        {
            CheckInstantiated();
            Instance.StartRecordingInternal(name, extraMetadata);
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

        public static void StartRecordingObject(IObjectSafeRef objectSafeRef, bool markCreated = true)
        {
            CheckInstantiated();
            Instance.StartRecordingObjectInternal(objectSafeRef, markCreated);
        }

        public static void StopRecordingObject(IObjectSafeRef objectSafeRef, bool markDestroyed = true)
        {
            CheckInstantiated();
            Instance.StopRecordingObjectInternal(objectSafeRef, markDestroyed);
        }

        // TODO: if game object, also start recording all components (?)
        public static void StartRecordingObject<T>(T obj, bool markCreated = true) where T : UnityEngine.Object
        {
            CheckInstantiated();
            var objectSafeRef = Instance._context.ObjectSafeRefProvider.GetOrCreateObjectSafeRef(obj);
            Instance.StartRecordingObjectInternal(objectSafeRef, markCreated);
        }

        // TODO: if game object, also stop recording all components (?)
        public static void StopRecordingObject<T>(T obj, bool markDestroyed = true) where T : UnityEngine.Object
        {
            CheckInstantiated();
            var objectSafeRef = Instance._context.ObjectSafeRefProvider.GetOrCreateObjectSafeRef(obj);
            Instance.StopRecordingObjectInternal(objectSafeRef, markDestroyed);
        }

        /// <summary>
        /// Instantiates the recorder instance after the assemblies are loaded by the application.
        /// This method is called automatically by the application.
        /// Recorder modules are found by scanning all the assemblies and are instantiated using their default constructor.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Instantiate()
        {
            var objSafeRefProvider = new ObjectSafeRefProvider();
            var recorderModules = RecorderModuleManager.InstantiateRecorderModulesFromAllAssemblies();
            var dataDispatcher = new DataDispatcher();
            var settingsProvider = new FileSettingsProvider();
            var recorderContext = new RecorderContext(Array.AsReadOnly(recorderModules), objSafeRefProvider, settingsProvider);
            
            Instance = new PlumeRecorder(dataDispatcher, recorderContext);

            foreach (var recorderModule in recorderModules)
            {
                recorderModule.Create(recorderContext);
            }

            ApplicationPauseDetector.Paused += () => Instance.OnApplicationPaused();

            Application.wantsToQuit += Instance.OnApplicationWantsToQuit;
            Application.quitting += () =>
            {
                Instance.OnApplicationQuitting();
                Instance.Dispose();
            };
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        internal static void OnSceneLoaded()
        {
            Instance.Awake();
        }
    }
}