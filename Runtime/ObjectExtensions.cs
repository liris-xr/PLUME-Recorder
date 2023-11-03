using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PLUME
{
    public static class ObjectExtensions
    {
        private static readonly MethodInfo FindObjectFromInstanceIDMethod =
            typeof(Object).GetMethod("FindObjectFromInstanceID", BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly Dictionary<int, Object> CachedObjectFromInstanceId = new();

        /**
         * Forces an invocation of OnCreate event. Should be called *after* the creation of the object.
         * 
         * Note that in most cases, this will not be necessary as the patcher already injects the notification in most
         * of the method used to instantiate Objects (GameObject constructor, AddComponent, etc).
         * 
         * One of the only un-patchable method which requires to use this ForceNotifyOnCreate is
         * GameObject.CreatePrimitive. To make sure that the newly created primitive is recorded, one would write:
         * \code{.cs}
         * var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
         * cube.ForceNotifyOnCreate();
         * \endcode
         */
        public static void ForceNotifyOnCreate(this Object obj)
        {
            ObjectEvents.OnCreate?.Invoke(obj);
        }
        
        public static void ForceNotifyOnCreate(this GameObject go, bool notifyOnCreateForComponents = true)
        {
            ObjectEvents.OnCreate?.Invoke(go);

            if (notifyOnCreateForComponents)
            {
                foreach (var component in go.GetComponents<Component>())
                {
                    ObjectEvents.OnCreate?.Invoke(component);
                }
            }
        }

        /**
         * Forces an invocation of OnDestroy event. Should be called *before* the destruction of the object and only
         * if the patcher wasn't able to inject the event call automatically. Otherwise the destruction sample might be recorded twice.
         *
         * Note that in most cases, this will not be necessary as the patcher already injects the notification in most
         * of the method used to destroy Objects (Destroy, DestroyImmediate).
         */
        public static void ForceNotifyOnDestroy(this Object obj)
        {
            ObjectEvents.OnDestroy?.Invoke(obj.GetInstanceID());
        }
        
        public static void ForceNotifyOnDestroy(this GameObject go, bool notifyOnDestroyForComponents = true)
        {
            if (notifyOnDestroyForComponents)
            {
                foreach (var component in go.GetComponents<Component>())
                {
                    ObjectEvents.OnDestroy?.Invoke(component.GetInstanceID());
                }
            }
            
            ObjectEvents.OnDestroy?.Invoke(go.GetInstanceID());
        }

        // TODO: this can be moved inside PlayerContext and optimized using a cache updated when a new identifier correspondence is registered
        public static Object FindObjectByInstanceID(int instanceId)
        {
            var found = CachedObjectFromInstanceId.TryGetValue(instanceId, out var obj);

            if (found)
            {
                if (obj == null)
                {
                    CachedObjectFromInstanceId.Remove(instanceId);
                }
                else
                {
                    return obj;
                }
            }

            obj = (Object) FindObjectFromInstanceIDMethod.Invoke(null, new object[] {instanceId});
            CachedObjectFromInstanceId.Add(instanceId, obj);
            
            //TODO : Manage when obj is null (give safe handle)
            
            return obj;
        }
        
        public static List<GameObject> GetObjectsInLayer(LayerMask layerMask, bool includeInactive = false)
        {
            var ret = new List<GameObject>();
            foreach (var t in Object.FindObjectsOfType<Transform>(includeInactive))
            {
                var isIncludedInLayerMask = layerMask.value == (layerMask.value | (1 << t.gameObject.layer));
                
                if (isIncludedInLayerMask)
                {
                    ret.Add(t.gameObject);
                }
            }

            return ret;
        }
    }
}