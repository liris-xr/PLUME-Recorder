using System;
using PLUME.Sample.Common;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityRuntimeGuid;
using Bounds = PLUME.Sample.Common.Bounds;
using CameraClearFlags = PLUME.Sample.Unity.CameraClearFlags;
using CameraType = PLUME.Sample.Unity.CameraType;
using Color = PLUME.Sample.Common.Color;
using DepthTextureMode = PLUME.Sample.Unity.DepthTextureMode;
using FogMode = PLUME.Sample.Unity.FogMode;
using LightmapData = PLUME.Sample.Unity.LightmapData;
using LightmapsMode = PLUME.Sample.Unity.LightmapsMode;
using LightShadowCasterMode = PLUME.Sample.Unity.LightShadowCasterMode;
using LightShadows = PLUME.Sample.Unity.LightShadows;
using LightShape = PLUME.Sample.Unity.LightShape;
using LightType = PLUME.Sample.Unity.LightType;
using Matrix4x4 = PLUME.Sample.Common.Matrix4x4;
using Quaternion = PLUME.Sample.Common.Quaternion;
using Rect = PLUME.Sample.Common.Rect;
using RenderingPath = PLUME.Sample.Unity.RenderingPath;
using TransparencySortMode = PLUME.Sample.Unity.TransparencySortMode;
using Vector2 = PLUME.Sample.Common.Vector2;
using Vector3 = PLUME.Sample.Common.Vector3;
using Vector4 = PLUME.Sample.Common.Vector4;

#if URP_ENABLED
using AntialiasingMode = PLUME.Sample.Unity.AntialiasingMode;
using AntialiasingQuality = PLUME.Sample.Unity.AntialiasingQuality;
#endif

namespace PLUME.Core.Utils.Sample
{
    public static class PayloadUtils
    {
        public static AssetIdentifier ToAssetIdentifierPayload(this UnityEngine.Object obj)
        {
            var guidRegistry = AssetsGuidRegistry.GetOrCreate();
            var guidRegistryEntry = guidRegistry.GetOrCreateEntry(obj);

            return new AssetIdentifier
            {
                Id = guidRegistryEntry.guid,
                Path = guidRegistryEntry.assetBundlePath
            };
        }
        
        public static ComponentIdentifier ToIdentifierPayload(this Component component)
        {
            var guidRegistry = SceneGuidRegistry.GetOrCreate(component.gameObject.scene);
            var guidRegistryEntry = guidRegistry.GetOrCreateEntry(component);

            return new ComponentIdentifier
            {
                Id = guidRegistryEntry.guid,
                ParentId = component.gameObject.ToIdentifierPayload()
            };
        }

        public static TransformGameObjectIdentifier ToIdentifierPayload(this GameObject go)
        {
            var guidRegistry = SceneGuidRegistry.GetOrCreate(go.scene);
            var gameObjectGuidRegistryEntry = guidRegistry.GetOrCreateEntry(go);
            var transformGuidRegistryEntry = guidRegistry.GetOrCreateEntry(go.transform);

            return new TransformGameObjectIdentifier
            {
                TransformId = transformGuidRegistryEntry.guid,
                GameObjectId = gameObjectGuidRegistryEntry.guid
            };
        }

        public static TransformGameObjectIdentifier ToIdentifierPayload(this Transform t)
        {
            return t.gameObject.ToIdentifierPayload();
        }

        public static Vector2 ToPayload(this UnityEngine.Vector2 vec)
        {
            return new Vector2
            {
                X = vec.x,
                Y = vec.y,
            };
        }

        public static Vector3 ToPayload(this UnityEngine.Vector3 vec)
        {
            return new Vector3
            {
                X = vec.x,
                Y = vec.y,
                Z = vec.z,
            };
        }

        public static Vector4 ToPayload(this UnityEngine.Vector4 vec)
        {
            return new Vector4
            {
                X = vec.x,
                Y = vec.y,
                Z = vec.z,
                W = vec.w
            };
        }

        public static Quaternion ToPayload(this UnityEngine.Quaternion vec)
        {
            return new Quaternion
            {
                X = vec.x,
                Y = vec.y,
                Z = vec.z,
                W = vec.w
            };
        }

        public static Color ToPayload(this UnityEngine.Color color)
        {
            return new Color
            {
                R = color.r,
                G = color.g,
                B = color.b,
                A = color.a
            };
        }
        
        public static Rect ToPayload(this UnityEngine.Rect rect)
        {
            return new Rect
            {
                X = rect.xMin,
                Y = rect.yMin,
                Width = rect.width,
                Height = rect.height
            };
        }

        public static SphericalHarmonicsL2 ToPayload(
            this UnityEngine.Rendering.SphericalHarmonicsL2 sphericalHarmonicsL2)
        {
            return new SphericalHarmonicsL2
            {
                Shr0 = sphericalHarmonicsL2[0, 0],
                Shr1 = sphericalHarmonicsL2[0, 1],
                Shr2 = sphericalHarmonicsL2[0, 2],
                Shr3 = sphericalHarmonicsL2[0, 3],
                Shr4 = sphericalHarmonicsL2[0, 4],
                Shr5 = sphericalHarmonicsL2[0, 5],
                Shr6 = sphericalHarmonicsL2[0, 6],
                Shr7 = sphericalHarmonicsL2[0, 7],
                Shr8 = sphericalHarmonicsL2[0, 8],
                Shg0 = sphericalHarmonicsL2[0, 9],
                Shg1 = sphericalHarmonicsL2[0, 10],
                Shg2 = sphericalHarmonicsL2[0, 11],
                Shg3 = sphericalHarmonicsL2[0, 12],
                Shg4 = sphericalHarmonicsL2[0, 13],
                Shg5 = sphericalHarmonicsL2[0, 14],
                Shg6 = sphericalHarmonicsL2[0, 15],
                Shg7 = sphericalHarmonicsL2[0, 16],
                Shg8 = sphericalHarmonicsL2[0, 17],
                Shb0 = sphericalHarmonicsL2[0, 18],
                Shb1 = sphericalHarmonicsL2[0, 19],
                Shb2 = sphericalHarmonicsL2[0, 20],
                Shb3 = sphericalHarmonicsL2[0, 21],
                Shb4 = sphericalHarmonicsL2[0, 22],
                Shb5 = sphericalHarmonicsL2[0, 23],
                Shb6 = sphericalHarmonicsL2[0, 24],
                Shb7 = sphericalHarmonicsL2[0, 25],
                Shb8 = sphericalHarmonicsL2[0, 26]
            };
        }

        public static LightType ToPayload(this UnityEngine.LightType lightType)
        {
            return lightType switch
            {
                UnityEngine.LightType.Point => LightType.Point,
                UnityEngine.LightType.Directional => LightType.Directional,
                UnityEngine.LightType.Spot => LightType.Spot,
                UnityEngine.LightType.Rectangle => LightType.Rectangle,
                UnityEngine.LightType.Disc => LightType.Disc,
                _ => throw new ArgumentOutOfRangeException(nameof(lightType), lightType, null)
            };
        }

        public static LightShape ToPayload(this UnityEngine.LightShape lightShape)
        {
            return lightShape switch
            {
                UnityEngine.LightShape.Cone => LightShape.Cone,
                UnityEngine.LightShape.Pyramid => LightShape.Pyramid,
                UnityEngine.LightShape.Box => LightShape.Box,
                _ => throw new ArgumentOutOfRangeException(nameof(lightShape), lightShape, null)
            };
        }

        public static LightShadows ToPayload(this UnityEngine.LightShadows lightShadows)
        {
            return lightShadows switch
            {
                UnityEngine.LightShadows.None => LightShadows.None,
                UnityEngine.LightShadows.Hard => LightShadows.Hard,
                UnityEngine.LightShadows.Soft => LightShadows.Soft,
                _ => throw new ArgumentOutOfRangeException(nameof(lightShadows), lightShadows, null)
            };
        }

        public static LightShadowResolution ToPayload(
            this UnityEngine.Rendering.LightShadowResolution lightShadowResolution)
        {
            return lightShadowResolution switch
            {
                UnityEngine.Rendering.LightShadowResolution.FromQualitySettings => LightShadowResolution
                    .FromQualitySettings,
                UnityEngine.Rendering.LightShadowResolution.Low => LightShadowResolution.Low,
                UnityEngine.Rendering.LightShadowResolution.Medium => LightShadowResolution.Medium,
                UnityEngine.Rendering.LightShadowResolution.High => LightShadowResolution.High,
                UnityEngine.Rendering.LightShadowResolution.VeryHigh => LightShadowResolution.VeryHigh,
                _ => throw new ArgumentOutOfRangeException(nameof(lightShadowResolution), lightShadowResolution, null)
            };
        }

        public static LightShadowCasterMode ToPayload(this UnityEngine.LightShadowCasterMode lightShadowCasterMode)
        {
            return lightShadowCasterMode switch
            {
                UnityEngine.LightShadowCasterMode.Default => LightShadowCasterMode.Default,
                UnityEngine.LightShadowCasterMode.NonLightmappedOnly => LightShadowCasterMode.NonLightmappedOnly,
                UnityEngine.LightShadowCasterMode.Everything => LightShadowCasterMode.Everything,
                _ => throw new ArgumentOutOfRangeException(nameof(lightShadowCasterMode), lightShadowCasterMode, null)
            };
        }

        public static FogMode ToPayload(this UnityEngine.FogMode fogMode)
        {
            return fogMode switch
            {
                UnityEngine.FogMode.Linear => FogMode.Linear,
                UnityEngine.FogMode.Exponential => FogMode.Exponential,
                UnityEngine.FogMode.ExponentialSquared => FogMode.ExponentialSquared,
                _ => throw new ArgumentOutOfRangeException(nameof(fogMode), fogMode, null)
            };
        }

        public static DefaultReflectionMode ToPayload(
            this UnityEngine.Rendering.DefaultReflectionMode defaultReflectionMode)
        {
            return defaultReflectionMode switch
            {
                UnityEngine.Rendering.DefaultReflectionMode.Skybox => DefaultReflectionMode.Skybox,
                UnityEngine.Rendering.DefaultReflectionMode.Custom => DefaultReflectionMode.Custom,
                _ => throw new ArgumentOutOfRangeException(nameof(defaultReflectionMode), defaultReflectionMode, null)
            };
        }

        public static AmbientMode ToPayload(this UnityEngine.Rendering.AmbientMode ambientMode)
        {
            return ambientMode switch
            {
                UnityEngine.Rendering.AmbientMode.Skybox => AmbientMode.Skybox,
                UnityEngine.Rendering.AmbientMode.Trilight => AmbientMode.Trilight,
                UnityEngine.Rendering.AmbientMode.Flat => AmbientMode.Flat,
                UnityEngine.Rendering.AmbientMode.Custom => AmbientMode.Custom,
                _ => throw new ArgumentOutOfRangeException(nameof(ambientMode), ambientMode, null)
            };
        }

        public static Bounds ToPayload(this UnityEngine.Bounds bounds)
        {
            return new Bounds
            {
                Center = bounds.center.ToPayload(),
                Extents = bounds.extents.ToPayload()
            };
        }

        public static LightmapData ToPayload(this UnityEngine.LightmapData lightmapData)
        {
            return new LightmapData
            {
                LightmapColorTextureId = lightmapData.lightmapColor == null ? null : lightmapData.lightmapColor.ToAssetIdentifierPayload(),
                LightmapDirTextureId = lightmapData.lightmapDir == null ? null : lightmapData.lightmapDir.ToAssetIdentifierPayload(),
                LightmapShadowMaskTextureId = lightmapData.shadowMask == null ? null : lightmapData.shadowMask.ToAssetIdentifierPayload()
            };
        }

        public static LightmapsMode ToPayload(this UnityEngine.LightmapsMode lightmapsMode)
        {
            return lightmapsMode switch
            {
                UnityEngine.LightmapsMode.NonDirectional => LightmapsMode.NonDirectional,
                UnityEngine.LightmapsMode.CombinedDirectional => LightmapsMode.CombinedDirectional,
                _ => throw new ArgumentOutOfRangeException(nameof(lightmapsMode), lightmapsMode, null)
            };
        }

        public static ReflectionProbeMode ToPayload(this UnityEngine.Rendering.ReflectionProbeMode reflectionProbeMode)
        {
            return reflectionProbeMode switch
            {
                UnityEngine.Rendering.ReflectionProbeMode.Custom => ReflectionProbeMode.Custom,
                UnityEngine.Rendering.ReflectionProbeMode.Baked => ReflectionProbeMode.Baked,
                UnityEngine.Rendering.ReflectionProbeMode.Realtime => ReflectionProbeMode.Realtime,
                _ => throw new ArgumentOutOfRangeException(nameof(reflectionProbeMode), reflectionProbeMode, null)
            };
        }

        public static ReflectionProbeRefreshMode ToPayload(
            this UnityEngine.Rendering.ReflectionProbeRefreshMode reflectionProbeRefreshMode)
        {
            return reflectionProbeRefreshMode switch
            {
                UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame => ReflectionProbeRefreshMode.EveryFrame,
                UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake => ReflectionProbeRefreshMode.OnAwake,
                UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting =>
                    ReflectionProbeRefreshMode.ViaScripting,
                _ => throw new ArgumentOutOfRangeException(nameof(reflectionProbeRefreshMode),
                    reflectionProbeRefreshMode, null)
            };
        }

        public static ReflectionProbeTimeSlicingMode ToPayload(
            this UnityEngine.Rendering.ReflectionProbeTimeSlicingMode reflectionProbeTimeSlicingMode)
        {
            return reflectionProbeTimeSlicingMode switch
            {
                UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.NoTimeSlicing => ReflectionProbeTimeSlicingMode
                    .NoTimeSlicing,
                UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.IndividualFaces => ReflectionProbeTimeSlicingMode
                    .IndividualFaces,
                UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.AllFacesAtOnce => ReflectionProbeTimeSlicingMode
                    .AllFacesAtOnce,
                _ => throw new ArgumentOutOfRangeException(nameof(reflectionProbeTimeSlicingMode),
                    reflectionProbeTimeSlicingMode, null)
            };
        }

        public static ReflectionProbeClearFlags ToPayload(
            this UnityEngine.Rendering.ReflectionProbeClearFlags reflectionProbeClearFlags)
        {
            return reflectionProbeClearFlags switch
            {
                UnityEngine.Rendering.ReflectionProbeClearFlags.Skybox => ReflectionProbeClearFlags.Skybox,
                UnityEngine.Rendering.ReflectionProbeClearFlags.SolidColor => ReflectionProbeClearFlags.SolidColor,
                _ => throw new ArgumentOutOfRangeException(nameof(reflectionProbeClearFlags), reflectionProbeClearFlags,
                    null)
            };
        }

        public static ShadowCastingMode ToPayload(this UnityEngine.Rendering.ShadowCastingMode shadowCastingMode)
        {
            return shadowCastingMode switch
            {
                UnityEngine.Rendering.ShadowCastingMode.Off => ShadowCastingMode.Off,
                UnityEngine.Rendering.ShadowCastingMode.On => ShadowCastingMode.On,
                UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly => ShadowCastingMode.ShadowsOnly,
                UnityEngine.Rendering.ShadowCastingMode.TwoSided => ShadowCastingMode.TwoSided,
                _ => throw new ArgumentOutOfRangeException(nameof(shadowCastingMode), shadowCastingMode,
                    null)
            };
        }
        
        public static ReflectionProbeUsage ToPayload(this UnityEngine.Rendering.ReflectionProbeUsage reflectionProbeUsage)
        {
            return reflectionProbeUsage switch
            {
                UnityEngine.Rendering.ReflectionProbeUsage.Off => ReflectionProbeUsage.Off,
                UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes => ReflectionProbeUsage.BlendProbes,
                UnityEngine.Rendering.ReflectionProbeUsage.BlendProbesAndSkybox => ReflectionProbeUsage.BlendProbesAndSkybox,
                UnityEngine.Rendering.ReflectionProbeUsage.Simple => ReflectionProbeUsage.Simple,
                _ => throw new ArgumentOutOfRangeException(nameof(reflectionProbeUsage), reflectionProbeUsage,
                    null)
            };
        }
        
        public static Matrix4x4 ToPayload(this UnityEngine.Matrix4x4 mtx)
        {
            return new Matrix4x4
            {
                M00 = mtx.m00,
                M10 = mtx.m10,
                M20 = mtx.m20,
                M30 = mtx.m30,
                M01 = mtx.m01,
                M11 = mtx.m11,
                M21 = mtx.m21,
                M31 = mtx.m31,
                M02 = mtx.m02,
                M12 = mtx.m12,
                M22 = mtx.m22,
                M32 = mtx.m32,
                M03 = mtx.m03,
                M13 = mtx.m13,
                M23 = mtx.m23,
                M33 = mtx.m33
            };
        }
        
        public static OpaqueSortMode ToPayload(this UnityEngine.Rendering.OpaqueSortMode opaqueSortMode)
        {
            return opaqueSortMode switch
            {
                UnityEngine.Rendering.OpaqueSortMode.Default => OpaqueSortMode.Default,
                UnityEngine.Rendering.OpaqueSortMode.NoDistanceSort => OpaqueSortMode.NoDistanceSort,
                UnityEngine.Rendering.OpaqueSortMode.FrontToBack => OpaqueSortMode.FrontToBack,
                _ => throw new ArgumentOutOfRangeException(nameof(opaqueSortMode), opaqueSortMode,
                    null)
            };
        }
        
        public static TransparencySortMode ToPayload(this UnityEngine.TransparencySortMode transparencySortMode)
        {
            return transparencySortMode switch
            {
                UnityEngine.TransparencySortMode.Default => TransparencySortMode.Default,
                UnityEngine.TransparencySortMode.Orthographic => TransparencySortMode.Orthographic,
                UnityEngine.TransparencySortMode.Perspective => TransparencySortMode.Perspective,
                UnityEngine.TransparencySortMode.CustomAxis => TransparencySortMode.CustomAxis,
                _ => throw new ArgumentOutOfRangeException(nameof(transparencySortMode), transparencySortMode,
                    null)
            };
        }
        
        public static RenderingPath ToPayload(this UnityEngine.RenderingPath renderingPath)
        {
            return renderingPath switch
            {
                UnityEngine.RenderingPath.DeferredShading => RenderingPath.DeferredShading,
                UnityEngine.RenderingPath.Forward => RenderingPath.Forward,
                UnityEngine.RenderingPath.VertexLit => RenderingPath.VertexLit,
                UnityEngine.RenderingPath.UsePlayerSettings => RenderingPath.UsePlayerSettings,
                _ => throw new ArgumentOutOfRangeException(nameof(renderingPath), renderingPath,
                    null)
            };
        }
        
        public static CameraType ToPayload(this UnityEngine.CameraType cameraType)
        {
            return cameraType switch
            {
                UnityEngine.CameraType.Reflection => CameraType.Reflection,
                UnityEngine.CameraType.Game => CameraType.Game,
                UnityEngine.CameraType.Preview => CameraType.Preview,
                UnityEngine.CameraType.SceneView => CameraType.SceneView,
                UnityEngine.CameraType.VR => CameraType.Vr,
                _ => throw new ArgumentOutOfRangeException(nameof(cameraType), cameraType,
                    null)
            };
        }
        
        public static CameraGateFitMode ToPayload(this Camera.GateFitMode gateFitMode)
        {
            return gateFitMode switch
            {
                Camera.GateFitMode.None => CameraGateFitMode.None,
                Camera.GateFitMode.Fill => CameraGateFitMode.Fill,
                Camera.GateFitMode.Horizontal => CameraGateFitMode.Horizontal,
                Camera.GateFitMode.Vertical => CameraGateFitMode.Vertical,
                Camera.GateFitMode.Overscan => CameraGateFitMode.Overscan,
                _ => throw new ArgumentOutOfRangeException(nameof(gateFitMode), gateFitMode,
                    null)
            };
        }
        public static CameraClearFlags ToPayload(this UnityEngine.CameraClearFlags cameraClearFlags)
        {
            return cameraClearFlags switch
            {
                UnityEngine.CameraClearFlags.Nothing => CameraClearFlags.Nothing,
                UnityEngine.CameraClearFlags.Skybox => CameraClearFlags.Skybox,
                UnityEngine.CameraClearFlags.SolidColor => CameraClearFlags.SolidColor,
                UnityEngine.CameraClearFlags.Depth => CameraClearFlags.Depth,
                _ => throw new ArgumentOutOfRangeException(nameof(cameraClearFlags), cameraClearFlags,
                    null)
            };
        }
        
        public static DepthTextureMode ToPayload(this UnityEngine.DepthTextureMode depthTextureMode)
        {
            return depthTextureMode switch
            {
                UnityEngine.DepthTextureMode.None => DepthTextureMode.None,
                UnityEngine.DepthTextureMode.Depth => DepthTextureMode.Depth,
                UnityEngine.DepthTextureMode.DepthNormals => DepthTextureMode.DepthNormals,
                UnityEngine.DepthTextureMode.MotionVectors => DepthTextureMode.MotionVectors,
                _ => throw new ArgumentOutOfRangeException(nameof(depthTextureMode), depthTextureMode,
                    null)
            };
        }
     
        public static CameraStereoTargetEyeMask ToPayload(this StereoTargetEyeMask stereoTargetEyeMask)
        {
            return stereoTargetEyeMask switch
            {
                StereoTargetEyeMask.None => CameraStereoTargetEyeMask.None,
                StereoTargetEyeMask.Both => CameraStereoTargetEyeMask.Both,
                StereoTargetEyeMask.Left => CameraStereoTargetEyeMask.Left,
                StereoTargetEyeMask.Right => CameraStereoTargetEyeMask.Right,
                _ => throw new ArgumentOutOfRangeException(nameof(stereoTargetEyeMask), stereoTargetEyeMask,
                    null)
            };
        }
        
        public static LoadSceneMode ToPayload(this UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
        {
            return loadSceneMode switch
            {
                UnityEngine.SceneManagement.LoadSceneMode.Additive => LoadSceneMode.Additive,
                UnityEngine.SceneManagement.LoadSceneMode.Single => LoadSceneMode.Single,
                _ => throw new ArgumentOutOfRangeException(nameof(loadSceneMode), loadSceneMode,null)
            };
        }

#if URP_ENABLED
        public static CameraOverrideOption ToPayload(this UnityEngine.Rendering.Universal.CameraOverrideOption cameraOverrideOption)
        {
            return cameraOverrideOption switch
            {
                UnityEngine.Rendering.Universal.CameraOverrideOption.Off => CameraOverrideOption.Off,
                UnityEngine.Rendering.Universal.CameraOverrideOption.On => CameraOverrideOption.On ,
                UnityEngine.Rendering.Universal.CameraOverrideOption.UsePipelineSettings => CameraOverrideOption.UsePipelineSettings,
                _ => throw new ArgumentOutOfRangeException(nameof(cameraOverrideOption), cameraOverrideOption,
                    null)
            };
        }
        
        public static CameraRenderType ToPayload(this UnityEngine.Rendering.Universal.CameraRenderType cameraRenderType)
        {
            return cameraRenderType switch
            {
                UnityEngine.Rendering.Universal.CameraRenderType.Base => CameraRenderType.Base,
                UnityEngine.Rendering.Universal.CameraRenderType.Overlay => CameraRenderType.Overlay,
                _ => throw new ArgumentOutOfRangeException(nameof(cameraRenderType), cameraRenderType,
                    null)
            };
        }
        
        public static AntialiasingMode ToPayload(this UnityEngine.Rendering.Universal.AntialiasingMode antialiasingMode)
        {
            return antialiasingMode switch
            {
                UnityEngine.Rendering.Universal.AntialiasingMode.None => AntialiasingMode.None,
                UnityEngine.Rendering.Universal.AntialiasingMode.FastApproximateAntialiasing => AntialiasingMode.FastApproximateAntialiasing,
                UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing => AntialiasingMode.SubpixelMorphologicalAntiAliasing,
                _ => throw new ArgumentOutOfRangeException(nameof(antialiasingMode), antialiasingMode,
                    null)
            };
        }
        
        public static AntialiasingQuality ToPayload(this UnityEngine.Rendering.Universal.AntialiasingQuality antialiasingQuality)
        {
            return antialiasingQuality switch
            {
                UnityEngine.Rendering.Universal.AntialiasingQuality.Low => AntialiasingQuality.Low,
                UnityEngine.Rendering.Universal.AntialiasingQuality.Medium => AntialiasingQuality.Medium,
                UnityEngine.Rendering.Universal.AntialiasingQuality.High => AntialiasingQuality.High,
                _ => throw new ArgumentOutOfRangeException(nameof(antialiasingQuality), antialiasingQuality,
                    null)
            };
        }
#endif
    }
}