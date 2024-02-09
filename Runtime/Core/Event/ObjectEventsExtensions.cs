using UnityEngine;

namespace PLUME.Core.Event
{
    public static class ObjectEventsExtensions
    {

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
        public static void ForceNotifyOnCreate(this UnityEngine.Object obj)
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
        public static void ForceNotifyOnDestroy(this UnityEngine.Object obj)
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
    }
}