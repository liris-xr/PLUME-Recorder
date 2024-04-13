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

        public static void SetSharedMeshPropertyAndNotify(MeshFilter meshFilter, Mesh sharedMesh)
        {
            var previousMesh = meshFilter.sharedMesh;
            meshFilter.sharedMesh = sharedMesh;
            if (previousMesh != sharedMesh)
            {
                OnSharedMeshChanged(meshFilter, sharedMesh);
            }
        }

        public static void SetMeshPropertyAndNotify(MeshFilter meshFilter, Mesh mesh)
        {
            // sharedMesh points to mesh if instantiated. As such, using sharedMesh allows for detecting
            // if the mesh goes from shared to instantiated or from one instance to another.
            var previousMesh = meshFilter.sharedMesh;
            meshFilter.mesh = mesh;
            if (previousMesh != mesh)
            {
                OnMeshChanged(meshFilter, mesh);
            }
        }

        public static Mesh GetMeshPropertyAndNotify(MeshFilter meshFilter)
        {
            // The mesh property is subject to change when queried for the first time.
            var previousMesh = meshFilter.sharedMesh;
            var mesh = meshFilter.mesh;
            if (previousMesh != mesh)
            {
                OnMeshChanged(meshFilter, mesh);
            }

            return mesh;
        }
    }
}