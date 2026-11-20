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
        internal ModuleFaults Faults { get; } = new();

        private bool _destroyedObjectsSweepRequested;

        /// <summary>
        /// Whether the given module should record: the recorder is recording and the module was not disabled after
        /// a failure. Module event handlers (hooks, Unity callbacks) must check this rather than
        /// <see cref="IsRecording"/>, so that a disabled module stops recording from them too.
        /// </summary>
        public bool IsRecordingWith(IRecorderModule module)
        {
            return IsRecording && !Faults.IsDisabled(module);
        }

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
            using var _ = ListPool<Component>.Get(out var tmpComponents);
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
        }

        internal void StopRecordingGameObjectInternal(GameObject go, bool markDestroyed = true)
        {
            using var _ = ListPool<Component>.Get(out var tmpComponents);
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
        }

        /// <summary>
        /// Asks for <see cref="StopRecordingDestroyedObjects"/> before the frame data is collected, e.g. when a module
        /// notices an object destroyed without the hooks seeing it.
        /// </summary>
        internal void RequestDestroyedObjectsSweep()
        {
            _destroyedObjectsSweepRequested = true;
        }

        internal void StopRecordingDestroyedObjectsIfRequested()
        {
            if (_destroyedObjectsSweepRequested)
                StopRecordingDestroyedObjects();
        }

        /// <summary>
        /// Objects destroyed without the hooks seeing it (scene unload, engine, unhooked code) are still recorded:
        /// stops recording them, marked destroyed. Visits every recorded object: only run when some were destroyed.
        /// </summary>
        internal void StopRecordingDestroyedObjects()
        {
            _destroyedObjectsSweepRequested = false;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Modules.Count; i++)
            {
                var module = Modules[i];
                if (module is not IObjectRecorderModule objectRecorderModule || Faults.IsDisabled(module))
                    continue;

                try
                {
                    objectRecorderModule.StopRecordingDestroyedObjects(this);
                }
                catch (System.Exception e)
                {
                    ModuleFaults.ReportObjectFailure(module, nameof(StopRecordingDestroyedObjects), e);
                }
            }
        }

        internal void StartRecordingObjectInternal(IObjectSafeRef objectSafeRef, bool markCreated)
        {
            // TODO: cache the module by object type Dictionary<Type, List<Module>>
            // TODO: module should be picked from most specific to most general, with only one module per type
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Modules.Count; i++)
            {
                var module = Modules[i];
                if (module is not IObjectRecorderModule objectRecorderModule || Faults.IsDisabled(module))
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

                if (module is not IObjectRecorderModule objectRecorderModule || Faults.IsDisabled(module))
                    continue;

                objectRecorderModule.StopRecordingObject(objectSafeRef, markDestroyed, this);
            }
        }
    }
}