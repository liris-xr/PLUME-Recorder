using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PLUME.Core.Settings
{
    [Serializable]
    public abstract class Settings : ScriptableObject
    {
        internal const string BasePath = "Settings/PLUME";

        private static readonly Dictionary<string, Settings> LoadedSettings = new();

        public virtual void OnValidate()
        {
        }

        internal abstract string GetSettingsWindowPath();

        internal static T GetOrCreateInternal<T>(string settingsSubPath) where T : Settings
        {
            var settingsPath = Path.Join(BasePath, settingsSubPath);

            if (LoadedSettings.TryGetValue(settingsPath, out var settings))
            {
                return (T)settings;
            }

            // ReSharper disable once Unity.UnknownResource
            settings = Resources.Load<T>(settingsPath);

            if (settings != null)
            {
                LoadedSettings[settingsPath] = settings;
                return (T)settings;
            }

            settings = CreateInstance<T>();
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
            LoadedSettings[settingsSubPath] = settings;
            return (T)settings;
        }
    }
}