using System;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PLUME
{
    public static class UnityObjectUtils
    {
        [CanBeNull] private static readonly Func<int, Object> FindObjectFromInstanceIdFunc;

        static UnityObjectUtils()
        {
            var methodInfo = typeof(Object)
                .GetMethod("FindObjectFromInstanceID",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            if (methodInfo == null)
                Debug.LogError("FindObjectFromInstanceID was not found in UnityEngine.Object");
            else
                FindObjectFromInstanceIdFunc =
                    (Func<int, Object>)Delegate.CreateDelegate(typeof(Func<int, Object>), methodInfo);
        }

        [CanBeNull]
        public static Object FindObjectFromInstanceID(int instanceID)
        {
            return FindObjectFromInstanceIdFunc?.Invoke(instanceID);
        }

        [CanBeNull]
        public static T FindObjectFromInstanceID<T>(int instanceID) where T : class
        {
            return FindObjectFromInstanceIdFunc?.Invoke(instanceID) as T;
        }
    }
}