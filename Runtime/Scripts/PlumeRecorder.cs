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
            Patcher.DoPatching();
        }
    }
}