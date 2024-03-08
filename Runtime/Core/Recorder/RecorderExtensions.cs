using System;
using System.Collections.Generic;
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
        
        private static readonly List<Component> _tmpComponents = new();

        private static void CheckInstantiated()
        {
            if (Instance == null)
                throw new InvalidOperationException("PLUME recorder instance is not created yet.");
        }
        
        public static void StartRecording(string name, bool recordAll = true, string extraMetadata = "")
        {
            CheckInstantiated();
            Instance.StartRecordingInternal(name, extraMetadata);

            if (!recordAll)
                return;
            
            foreach (var go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                StartRecordingGameObject(go);
            }
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

        public static void StartRecordingGameObject(GameObject go, bool markCreated = true)
        {
            CheckInstantiated();
            var safeRefProvider = Instance._context.ObjectSafeRefProvider;
            
            go.GetComponentsInChildren(true, _tmpComponents);
            
            foreach (var component in _tmpComponents)
            {
                var componentSafeRef = safeRefProvider.GetOrCreateComponentSafeRef(component);

                // Start recording nested GameObjects. This also applies to the given GameObject itself.
                if (component is Transform)
                {
                    Instance.StartRecordingObjectInternal(componentSafeRef.GameObjectSafeRef, markCreated);
                }
                
                Instance.StartRecordingObjectInternal(componentSafeRef, markCreated);
            }
        }

        public static void StopRecordingGameObject(GameObject go, bool markDestroyed = true)
        {
            CheckInstantiated();
            var safeRefProvider = Instance._context.ObjectSafeRefProvider;
            
            go.GetComponentsInChildren(_tmpComponents);
            
            foreach (var component in _tmpComponents)
            {
                var componentSafeRef = safeRefProvider.GetOrCreateComponentSafeRef(component);
                Instance.StopRecordingObjectInternal(componentSafeRef, markDestroyed);
                
                // Stop recording nested GameObjects. This also applies to the given GameObject itself.
                if (component is Transform)
                {
                    Instance.StopRecordingObjectInternal(componentSafeRef.GameObjectSafeRef, markDestroyed);
                }
            }
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