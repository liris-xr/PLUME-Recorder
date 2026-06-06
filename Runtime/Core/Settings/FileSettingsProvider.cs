using System;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace PLUME.Core.Settings
{
    public class FileSettingsProvider : ISettingsProvider
    {
        public const string BasePath = "Settings/PLUME";

        private static readonly Dictionary<Type, Settings> LoadedSettings = new();

        public T GetOrCreate<T>() where T : Settings
        {
            var settingsType = typeof(T);

            if (LoadedSettings.TryGetValue(settingsType, out var settings))
            {
                return (T)settings;
            }

            var instance = ScriptableObject.CreateInstance<T>();

            // Resources.Load expects a '/'-separated path relative to a Resources
            // folder, without the file extension.
            var resourcePath = BasePath + "/" + instance.GetSettingsFileName();

            // ReSharper disable once Unity.UnknownResource
            var loadedSettings = Resources.Load<T>(resourcePath);

            if (loadedSettings != null)
            {
                DestroyInstance(instance);
                LoadedSettings[settingsType] = loadedSettings;
                return loadedSettings;
            }

            // No asset on disk yet. Cache and return the in-memory instance so that
            // callers running during a domain reload (e.g. [InitializeOnLoadMethod])
            // get a usable object immediately.
            LoadedSettings[settingsType] = instance;

#if UNITY_EDITOR
            // Persisting the asset touches the AssetDatabase. Doing that while Unity is
            // still importing/compiling (which is the case during InitializeOnLoad) makes
            // the import of the freshly created asset fail:
            //   "Unable to import newly created asset : .../HooksSettings.asset".
            // Defer it until the editor is idle and the asset pipeline is ready.
            ScheduleAssetCreation(instance);
#endif
            return instance;
        }

        private static void DestroyInstance(ScriptableObject instance)
        {
            if (instance == null)
                return;
#if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(instance);
#else
            UnityEngine.Object.Destroy(instance);
#endif
        }

#if UNITY_EDITOR
        private static void ScheduleAssetCreation<T>(T settings) where T : Settings
        {
            void Create()
            {
                // Right after a domain reload the pipeline can still be busy; retry next tick.
                if (EditorApplication.isUpdating || EditorApplication.isCompiling)
                {
                    EditorApplication.delayCall += Create;
                    return;
                }

                // The instance may have been destroyed by a domain reload before this ran.
                if (settings == null)
                    return;

                var assetPath = "Assets/Resources/" + BasePath + "/" +
                                settings.GetSettingsFileName() + ".asset";

                // A previous session or import may already have created the asset.
                if (AssetDatabase.LoadAssetAtPath<T>(assetPath) != null)
                    return;

                var absoluteAssetPath = Path.Combine(
                    Application.dataPath, "Resources", BasePath,
                    settings.GetSettingsFileName() + ".asset");
                var directory = Path.GetDirectoryName(absoluteAssetPath);
                if (directory != null && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                AssetDatabase.CreateAsset(settings, assetPath);
                AssetDatabase.SaveAssets();
            }

            EditorApplication.delayCall += Create;
        }
#endif
    }
}
