using System;
using PLUME.Core;
using UnityEngine;

namespace PLUME.Base.Hooks
{
    public static class MeshRendererHooks
    {
        public static Action<MeshRenderer, Mesh> OnSetAdditionalVertexStreams;

        public static Action<MeshRenderer, Mesh> OnSetEnlightenVertexStream;
        
        public static Action<MeshRenderer, float> OnSetScaleInLightmap;
        
        public static Action<MeshRenderer, ReceiveGI> OnSetReceiveGI;
        
        public static Action<MeshRenderer, bool> OnSetStitchLightmapSeams;

        [RegisterPropertySetterHook(typeof(MeshRenderer), nameof(MeshRenderer.additionalVertexStreams))]
        public static void SetAdditionalVertexStreamsHook(MeshRenderer meshRenderer, Mesh additionalVertexStreams)
        {
            OnSetAdditionalVertexStreams?.Invoke(meshRenderer, additionalVertexStreams);
        }

        [RegisterPropertySetterHook(typeof(MeshRenderer), nameof(MeshRenderer.enlightenVertexStream))]
        public static void SetEnlightenVertexStreamHook(MeshRenderer meshRenderer, Mesh enlightenVertexStream)
        {
            OnSetEnlightenVertexStream?.Invoke(meshRenderer, enlightenVertexStream);
        }
        
        [RegisterPropertySetterHook(typeof(MeshRenderer), "scaleInLightmap")]
        public static void SetScaleInLightmapHook(MeshRenderer meshRenderer, float scaleInLightmap)
        {
            OnSetScaleInLightmap?.Invoke(meshRenderer, scaleInLightmap);
        }
        
        [RegisterPropertySetterHook(typeof(MeshRenderer), "receiveGI")]
        public static void SetReceiveGIHook(MeshRenderer meshRenderer, ReceiveGI receiveGI)
        {
            OnSetReceiveGI?.Invoke(meshRenderer, receiveGI);
        }
        
        [RegisterPropertySetterHook(typeof(MeshRenderer), "stitchLightmapSeams")]
        public static void SetStitchLightmapSeamsHook(MeshRenderer meshRenderer, bool stitchLightmapSeams)
        {
            OnSetStitchLightmapSeams?.Invoke(meshRenderer, stitchLightmapSeams);
        }
    }
}