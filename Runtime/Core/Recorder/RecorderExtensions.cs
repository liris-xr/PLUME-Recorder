using System;
using Cysharp.Threading.Tasks;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Writer;
using PLUME.Core.Scripts;
using PLUME.Core.Utils;
using PLUME.Sample.Common;
using UnityEngine;

namespace PLUME.Core.Recorder
{
    public sealed partial class PlumeRecorder
    {
        public static PlumeRecorder Instance { get; private set; }
        
        public static RecorderStatus Status => Instance._status;

        public static bool IsRecording => Instance._status is RecorderStatus.Recording;

        public static bool IsStopping => Instance._status is RecorderStatus.Stopping;

        public static bool IsStopped => Instance._status is RecorderStatus.Stopped;

        private static void CheckInstantiated()
        {
            if (Instance == null)
                throw new InvalidOperationException("PLUME recorder instance is not created yet.");
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
            Instance.EnsureIsRecording();
            var marker = new Marker { Label = label };
            Instance._record.RecordTimestampedSample(marker);
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
            var objectSafeRef = Instance._context.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(obj);
            Instance.StartRecordingObjectInternal(objectSafeRef, markCreated);
        }

        public static void StopRecordingObject<T>(T obj, bool markDestroyed = true) where T : UnityEngine.Object
        {
            CheckInstantiated();
            var objectSafeRef = Instance._context.ObjectSafeRefProvider.GetOrCreateTypedObjectSafeRef(obj);
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
            var recorderContext = new RecorderContext(Array.AsReadOnly(recorderModules), objSafeRefProvider);

            Instance = new PlumeRecorder(dataDispatcher, recorderContext);

            foreach (var recorderModule in recorderModules)
            {
                recorderModule.Create(recorderContext);
            }
            
            PlayerLoopUtils.InjectPreUpdate<RecorderPreUpdate>(Instance.PreUpdate);
            PlayerLoopUtils.InjectUpdate<RecorderUpdate>(Instance.Update);
            PlayerLoopUtils.InjectEarlyUpdate<RecorderEarlyUpdate>(Instance.EarlyUpdate);
            PlayerLoopUtils.InjectPreLateUpdate<RecorderPreLateUpdate>(Instance.PreLateUpdate);
            PlayerLoopUtils.InjectPostLateUpdate<RecorderPostLateUpdate>(Instance.PostLateUpdate);

            ApplicationPauseDetector.Paused += () => Instance.OnApplicationPaused();

            Application.wantsToQuit += Instance.OnApplicationWantsToQuit;
            Application.quitting += () =>
            {
                Instance.OnApplicationQuitting();
                Instance.Dispose();
            };
        }
    }
}