using System;
using PLUME.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Hooks
{
    public static class MeshFilterHooks
    {
        public static Action<MeshFilter, Mesh> OnSetMesh;

        public static Action<MeshFilter, Mesh> OnSetSharedMesh;

        public static Action<MeshFilter, Mesh> OnGetMesh;

        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(MeshFilter), nameof(MeshFilter.sharedMesh))]
        public static void SetSharedMeshHook(MeshFilter meshFilter, Mesh sharedMesh)
        {
            OnSetSharedMesh?.Invoke(meshFilter, sharedMesh);
        }

        [Preserve]
        [RegisterHookAfterPropertySetter(typeof(MeshFilter), nameof(MeshFilter.mesh))]
        public static void SetMeshHook(MeshFilter meshFilter, Mesh mesh)
        {
            OnSetMesh?.Invoke(meshFilter, mesh);
        }
        
        [Preserve]
        [RegisterHookAfterPropertyGetter(typeof(MeshFilter), nameof(MeshFilter.mesh))]
        public static void GetMeshHook(MeshFilter meshFilter, Mesh mesh)
        {
            OnGetMesh?.Invoke(meshFilter, mesh);
        }
    }
}