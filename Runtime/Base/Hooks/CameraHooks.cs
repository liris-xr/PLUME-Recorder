using System;
using PLUME.Sample.Unity;
using UnityEngine;
using CameraClearFlags = UnityEngine.CameraClearFlags;
using CameraType = UnityEngine.CameraType;
using DepthTextureMode = UnityEngine.DepthTextureMode;
using RenderingPath = UnityEngine.RenderingPath;

namespace PLUME.Base.Hooks
{
    public class CameraHooks
    {
        public static Action<Camera, float> OnSetNearClipPlane;
        public static Action<Camera, float> OnSetFarClipPlane;
        public static Action<Camera, float> OnSetFieldOfView;
        public static Action<Camera, RenderingPath> OnSetRenderingPath;
        public static Action<Camera, bool> OnSetAllowHDR;
        public static Action<Camera, bool> OnSetAllowMSAA;
        public static Action<Camera, bool> OnSetAllowDynamicResolution;
        public static Action<Camera, bool> OnSetForceIntoRenderTexture;
        public static Action<Camera, float> OnSetOrthographicSize;
        public static Action<Camera, bool> OnSetOrthographic;
        public static Action<Camera, bool> OnSetOpaqueSortMode;
        public static Action<Camera, bool> OnSetTransparencySortMode;
        public static Action<Camera, Vector3> OnSetTransparencySortAxis;
        public static Action<Camera, float> OnSetDepth;
        public static Action<Camera, float> OnSetAspect;
        public static Action<Camera, int> OnSetCullingMask;
        public static Action<Camera, int> OnSetEventMask;
        public static Action<Camera, bool> OnSetLayerCullSpherical;
        public static Action<Camera, CameraType> OnSetCameraType;
        public static Action<Camera, CameraLayerCullDistances> OnSetLayerCullDistances;
        public static Action<Camera, bool> OnSetUseOcclusionCulling;
        public static Action<Camera, Matrix4x4> OnSetOcclusionCullingMatrix;
        public static Action<Camera, Color> OnSetBackgroundColor;
        public static Action<Camera, CameraClearFlags> OnSetClearFlags;
        public static Action<Camera, DepthTextureMode> OnSetDepthTextureMode;
        public static Action<Camera, bool> OnSetClearStencilAfterLightingPass;
        public static Action<Camera, bool> OnSetUsePhysicalProperties;
        public static Action<Camera, Vector2> OnSetSensorSize;
        public static Action<Camera, Vector2> OnSetLensShift;
        public static Action<Camera, float> OnSetFocalLength;
        public static Action<Camera, CameraGateFitMode> OnSetGateFit;
        public static Action<Camera, Rect> OnSetRect;
        public static Action<Camera, float> OnSetAperture;
        public static Action<Camera, float> OnSetFocalDistance;
        public static Action<Camera, float> OnSetTargetTexture;
        public static Action<Camera, float> OnSetTargetDisplay;
        public static Action<Camera, float> OnSetTargetEye;
        public static Action<Camera, float> OnSetStereoSeparation;
        public static Action<Camera, float> OnSetStereoConvergence;

    }
}