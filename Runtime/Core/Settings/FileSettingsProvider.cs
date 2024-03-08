using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
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
            
            settings = ScriptableObject.CreateInstance<T>();
            
            var settingsPath = Path.Join(BasePath, settings.GetSettingsFileName());

            // ReSharper disable once Unity.UnknownResource
            var loadedSettings = Resources.Load<T>(settingsPath);

            if (loadedSettings != null)
            {
                LoadedSettings[settingsType] = loadedSettings;
                return loadedSettings;
            }
            
#if UNITY_EDITOR
            var assetPath = Path.Join("Resources", settingsPath + ".asset");
            var directory = Path.GetDirectoryName(Path.Join(Application.dataPath, assetPath));
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            AssetDatabase.CreateAsset(settings, Path.Join("Assets", assetPath));
            AssetDatabase.SaveAssets();
#endif
            LoadedSettings[settingsType] = settings;
            return (T)settings;
        }
    }
}