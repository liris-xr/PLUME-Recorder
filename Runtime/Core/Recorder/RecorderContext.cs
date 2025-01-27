using System.Collections.ObjectModel;
using System.Linq;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Settings;
using UnityEngine;
using UnityEngine.Pool;

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
        
        internal void StartRecordingGameObjectInternal(GameObject go, bool markCreated = true)
        {
            var tmpComponents = ListPool<Component>.Get();
            go.GetComponentsInChildren(true, tmpComponents);

            foreach (var component in tmpComponents)
            {
                var componentSafeRef = SafeRefProvider.GetOrCreateComponentSafeRef(component);

                // Start recording nested GameObjects. This also applies to the given GameObject itself.
                if (component is Transform)
                {
                    StartRecordingObjectInternal(componentSafeRef.GameObjectSafeRef, markCreated);
                }

                StartRecordingObjectInternal(componentSafeRef, markCreated);
            }
            
            ListPool<Component>.Release(tmpComponents);
        }

        internal void StopRecordingGameObjectInternal(GameObject go, bool markDestroyed = true)
        {
            var tmpComponents = ListPool<Component>.Get();
            go.GetComponentsInChildren(tmpComponents);

            foreach (var component in tmpComponents)
            {
                var componentSafeRef = SafeRefProvider.GetOrCreateComponentSafeRef(component);
                StopRecordingObjectInternal(componentSafeRef, markDestroyed);

                // Stop recording nested GameObjects. This also applies to the given GameObject itself.
                if (component is Transform)
                {
                    StopRecordingObjectInternal(componentSafeRef.GameObjectSafeRef, markDestroyed);
                }
            }
            
            ListPool<Component>.Release(tmpComponents);
        }

        internal void StartRecordingObjectInternal(IObjectSafeRef objectSafeRef, bool markCreated)
        {
            // TODO: cache the module by object type Dictionary<Type, List<Module>>
            // TODO: module should be picked from most specific to most general, with only one module per type
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Modules.Count; i++)
            {
                var module = Modules[i];
                if (module is not IObjectRecorderModule objectRecorderModule)
                    continue;

                objectRecorderModule.StartRecordingObject(objectSafeRef, markCreated, this);
            }
        }

        internal void StopRecordingObjectInternal(IObjectSafeRef objectSafeRef, bool markDestroyed)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Modules.Count; i++)
            {
                var module = Modules[i];

                if (module is not IObjectRecorderModule objectRecorderModule)
                    continue;

                objectRecorderModule.StopRecordingObject(objectSafeRef, markDestroyed, this);
            }
        }
    }
}