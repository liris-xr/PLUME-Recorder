using PLUME.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Events
{
    public static class MeshFilterEvents
    {
        public delegate void OnMeshChangedDelegate(MeshFilter meshFilter, Mesh mesh);

        public static event OnMeshChangedDelegate OnMeshChanged = delegate { };
        public static event OnMeshChangedDelegate OnSharedMeshChanged = delegate { };

        [Preserve]
        [RegisterPropertySetterDetour(typeof(MeshFilter), nameof(MeshFilter.sharedMesh))]
        public static void SetSharedMeshPropertyAndNotify(MeshFilter meshFilter, Mesh sharedMesh)
        {
            var previousMesh = meshFilter.sharedMesh;
            meshFilter.sharedMesh = sharedMesh;
            if (previousMesh != sharedMesh)
            {
                OnSharedMeshChanged(meshFilter, sharedMesh);
            }
        }

        [Preserve]
        [RegisterPropertySetterDetour(typeof(MeshFilter), nameof(MeshFilter.mesh))]
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
        
        [Preserve]
        [RegisterPropertyGetterDetour(typeof(MeshFilter), nameof(MeshFilter.mesh))]
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