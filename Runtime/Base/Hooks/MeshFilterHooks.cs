using System;
using PLUME.Core;
using UnityEngine;

namespace PLUME.Base.Hooks
{
    public static class MeshFilterHooks
    {
        public static Action<MeshFilter, Mesh> OnSetMesh;
        
        public static Action<MeshFilter, Mesh> OnSetSharedMesh;
        
        [RegisterPropertySetterHook(typeof(MeshFilter), nameof(MeshFilter.sharedMesh))]
        public static void SetSharedMeshHook(MeshFilter meshFilter, Mesh sharedMesh)
        {
            OnSetSharedMesh?.Invoke(meshFilter, sharedMesh);
        }
        
        [RegisterPropertySetterHook(typeof(MeshFilter), nameof(MeshFilter.mesh))]
        public static void SetMeshHook(MeshFilter meshFilter, Mesh mesh)
        {
            OnSetMesh?.Invoke(meshFilter, mesh);
        }
    }
}