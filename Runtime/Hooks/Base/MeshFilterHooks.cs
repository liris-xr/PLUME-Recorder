using System;
using PLUME.Core.Hooks;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Hooks
{
    [Preserve]
    public class MeshFilterHooks : IRegisterHooksCallback
    {
        public delegate void OnMeshChangedDelegate(MeshFilter meshFilter, Mesh mesh);

        public static event OnMeshChangedDelegate OnMeshChanged = delegate { };
        public static event OnMeshChangedDelegate OnSharedMeshChanged = delegate { };

        public void RegisterHooks(HooksRegistry hooksRegistry)
        {
            hooksRegistry.RegisterHook(typeof(MeshFilterHooks).GetMethod(nameof(SetSharedMeshPropertyAndNotify)),
                typeof(MeshFilter).GetProperty(nameof(MeshFilter.sharedMesh))!.GetSetMethod());

            hooksRegistry.RegisterHook(typeof(MeshFilterHooks).GetMethod(nameof(SetMeshPropertyAndNotify)),
                typeof(MeshFilter).GetProperty(nameof(MeshFilter.mesh))!.GetSetMethod());

            hooksRegistry.RegisterHook(typeof(MeshFilterHooks).GetMethod(nameof(GetMeshPropertyAndNotify)),
                typeof(MeshFilter).GetProperty(nameof(MeshFilter.mesh))!.GetGetMethod());
        }

        internal static void NotifyMeshChanged(MeshFilter meshFilter, Mesh mesh)
        {
            try
            {
                OnMeshChanged(meshFilter, mesh);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(MeshFilterHooks)}.{nameof(OnMeshChanged)}",
                    "Failed to record a mesh change. The application is not affected.", e);
            }
        }

        internal static void NotifySharedMeshChanged(MeshFilter meshFilter, Mesh mesh)
        {
            try
            {
                OnSharedMeshChanged(meshFilter, mesh);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(MeshFilterHooks)}.{nameof(OnSharedMeshChanged)}",
                    "Failed to record a shared mesh change. The application is not affected.", e);
            }
        }

        public static void SetSharedMeshPropertyAndNotify(MeshFilter meshFilter, Mesh sharedMesh)
        {
            // A null or destroyed mesh filter is left to the user's call, so that Unity raises its own exception (same
            // below).
            if (meshFilter == null)
            {
                meshFilter.sharedMesh = sharedMesh;
                return;
            }

            var previousMesh = meshFilter.sharedMesh;
            meshFilter.sharedMesh = sharedMesh;
            if (previousMesh != sharedMesh)
            {
                NotifySharedMeshChanged(meshFilter, sharedMesh);
            }
        }

        public static void SetMeshPropertyAndNotify(MeshFilter meshFilter, Mesh mesh)
        {
            // sharedMesh points to mesh if instantiated. As such, using sharedMesh allows for detecting
            // if the mesh goes from shared to instantiated or from one instance to another.
            if (meshFilter == null)
            {
                meshFilter.mesh = mesh;
                return;
            }

            var previousMesh = meshFilter.sharedMesh;
            meshFilter.mesh = mesh;
            if (previousMesh != mesh)
            {
                NotifyMeshChanged(meshFilter, mesh);
            }
        }

        public static Mesh GetMeshPropertyAndNotify(MeshFilter meshFilter)
        {
            // The mesh property is subject to change when queried for the first time.
            if (meshFilter == null)
                return meshFilter.mesh;

            var previousMesh = meshFilter.sharedMesh;
            var mesh = meshFilter.mesh;
            if (previousMesh != mesh)
            {
                NotifyMeshChanged(meshFilter, mesh);
            }

            return mesh;
        }
    }
}