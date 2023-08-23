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

#if UNITY_STANDALONE_WIN
            Patcher.DoPatching();
#else
            // Android platforms can't be patched using Harmony because of IL2CPP
            var go = new GameObject();
            go.name = "Android event dispatcher";
            Object.DontDestroyOnLoad(go);
#endif
        }
    }
}