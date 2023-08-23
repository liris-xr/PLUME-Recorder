using System.Globalization;
using System.Threading;
using PLUME.Sample;
using UnityEngine;

namespace PLUME
{
    public static class Plume
    {
        public static readonly Version Version = new()
        {
            Name = "PLUME",
            Major = "alpha-1",
            Minor = "0",
            Patch = "0"
        };

        [RuntimeInitializeOnLoadMethod]
        public static void OnInitialize()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            
            // Android platforms can't be patched using Harmony because of IL2CPP
            if (Application.platform == RuntimePlatform.Android)
            {
                var go = new GameObject();
                go.name = "Android event dispatcher";
                Object.DontDestroyOnLoad(go);
            }
            else
            {
                Patcher.DoPatching();
            }
        }
    }
}