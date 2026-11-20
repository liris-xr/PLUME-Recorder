using System;
using Google.Protobuf.Reflection;
using PLUME.Core.Object.SafeRef;
using PLUME.Sample.Common;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.Settings;
using PLUME.Sample.Unity.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityRuntimeGuid;
using AnimationCurve = PLUME.Sample.Common.AnimationCurve;
using Bounds = PLUME.Sample.Common.Bounds;
using Color = PLUME.Sample.Common.Color;
using ColorSpace = PLUME.Sample.Common.ColorSpace;
using FogMode = PLUME.Sample.Unity.Settings.FogMode;
using FontStyle = PLUME.Sample.Unity.UI.FontStyle;
using HorizontalWrapMode = PLUME.Sample.Unity.UI.HorizontalWrapMode;
using LightmapData = PLUME.Sample.Unity.LightmapData;
using LightmapsMode = PLUME.Sample.Unity.LightmapsMode;
using LightShadowCasterMode = PLUME.Sample.Unity.LightShadowCasterMode;
using LightShadows = PLUME.Sample.Unity.LightShadows;
using LightShape = PLUME.Sample.Unity.LightShape;
using LightType = PLUME.Sample.Unity.LightType;
using LightUnit = PLUME.Sample.Unity.LightUnit;
using LoadSceneMode = PLUME.Sample.Unity.LoadSceneMode;
using Matrix4x4 = PLUME.Sample.Common.Matrix4x4;
using Quaternion = PLUME.Sample.Common.Quaternion;
using Rect = PLUME.Sample.Common.Rect;
using RenderingPath = PLUME.Sample.Unity.RenderingPath;
using RenderMode = PLUME.Sample.Unity.UI.RenderMode;
using ScaleMode = PLUME.Sample.Unity.UI.ScaleMode;
using StandaloneRenderResize = PLUME.Sample.Unity.UI.StandaloneRenderResize;
using TextAnchor = PLUME.Sample.Unity.UI.TextAnchor;
using TransparencySortMode = PLUME.Sample.Unity.TransparencySortMode;
using Vector2 = PLUME.Sample.Common.Vector2;
using Vector3 = PLUME.Sample.Common.Vector3;
using Vector4 = PLUME.Sample.Common.Vector4;
using VerticalWrapMode = PLUME.Sample.Unity.UI.VerticalWrapMode;
using WeightedMode = PLUME.Sample.Common.WeightedMode;

#if URP_ENABLED
using CameraOverrideOption = PLUME.Sample.Unity.URP.CameraOverrideOption;
using CameraRenderType = PLUME.Sample.Unity.URP.CameraRenderType;
using AntialiasingMode = PLUME.Sample.Unity.URP.AntialiasingMode;
using AntialiasingQuality = PLUME.Sample.Unity.URP.AntialiasingQuality;
#endif

namespace PLUME.Core.Utils
{
    public static class SampleUtils
    {
        public static readonly string NullGuid = Guid.Null.ToString();

        public static readonly AssetIdentifier NullAssetIdentifierPayload =
            new() { Guid = NullGuid, AssetBundlePath = "" };

        public static readonly SceneIdentifier NullSceneIdentifierPayload = new()
        {
            Guid = NullGuid,
            Name = ""
        };

        public static readonly GameObjectIdentifier NullGameObjectIdentifierPayload = new()
        {
            Guid = NullGuid,
            TransformGuid = NullGuid,
            Scene = NullSceneIdentifierPayload
        };

        public static readonly ComponentIdentifier NullComponentIdentifierPayload = new()
        {
            Guid = NullGuid,
            GameObject = NullGameObjectIdentifierPayload
        };

        public static AssetIdentifier GetAssetIdentifierPayload(UnityEngine.Object obj)
        {
            if (obj == null)
                return NullAssetIdentifierPayload;

            var guidRegistry = AssetsGuidRegistry.GetOrCreate();
            var guidRegistryEntry = guidRegistry.GetOrCreateEntry(obj);

            return new AssetIdentifier
            {
                Guid = guidRegistryEntry.guid,
                AssetBundlePath = guidRegistryEntry.assetBundlePath
            };
        }

        public static AssetIdentifier GetAssetIdentifierPayload(IAssetSafeRef asset)
        {
            if (asset.IsNull)
            {
                return NullAssetIdentifierPayload;
            }

            return new AssetIdentifier
            {
                Guid = asset.Identifier.Guid.ToString(),
                AssetBundlePath = asset.Identifier.AssetPath.ToString()
            };
        }

        public static ComponentIdentifier GetComponentIdentifierPayload(Component component)
        {
            if (component == null)
                return NullComponentIdentifierPayload;

            var guidRegistry = SceneGuidRegistry.GetOrCreate(component.gameObject.scene);
            var componentGuidRegistryEntry = guidRegistry.GetOrCreateEntry(component);

            return new ComponentIdentifier
            {
                Guid = componentGuidRegistryEntry.guid,
                GameObject = GetGameObjectIdentifierPayload(component.gameObject)
            };
        }

        public static ComponentIdentifier GetComponentIdentifierPayload(IComponentSafeRef component)
        {
            if (component.IsNull)
            {
                return NullComponentIdentifierPayload;
            }

            return new ComponentIdentifier
            {
                Guid = component.Identifier.Guid.ToString(),
                GameObject = new GameObjectIdentifier
                {
                    Guid = component.Identifier.GameObject.Guid.ToString(),
                    TransformGuid = component.GameObjectSafeRef.Identifier.TransformGuid.ToString(),
                    Scene = GetSceneIdentifierPayload(component.GameObjectSafeRef.SceneSafeRef)
                }
            };
        }

        public static GameObjectIdentifier GetGameObjectIdentifierPayload(GameObject go)
        {
            if (go == null)
                return NullGameObjectIdentifierPayload;

            var guidRegistry = SceneGuidRegistry.GetOrCreate(go.scene);
            var gameObjectGuidRegistryEntry = guidRegistry.GetOrCreateEntry(go);
            var transformGuidRegistryEntry = guidRegistry.GetOrCreateEntry(go.transform);

            return new GameObjectIdentifier
            {
                Guid = gameObjectGuidRegistryEntry.guid,
                TransformGuid = transformGuidRegistryEntry.guid,
                Scene = GetSceneIdentifierPayload(go.scene)
            };
        }

        public static GameObjectIdentifier GetGameObjectIdentifierPayload(GameObjectSafeRef go)
        {
            if (go.IsNull)
            {
                return NullGameObjectIdentifierPayload;
            }

            return new GameObjectIdentifier
            {
                Guid = go.Identifier.Guid.ToString(),
                TransformGuid = go.Identifier.TransformGuid.ToString(),
                Scene = GetSceneIdentifierPayload(go.SceneSafeRef)
            };
        }

        public static SceneIdentifier GetSceneIdentifierPayload(Scene scene)
        {
            var guidRegistry = SceneGuidRegistry.GetOrCreate(scene);

            var sceneIdentifier = new SceneIdentifier
            {
                Guid = guidRegistry.SceneGuid,
                AssetBundlePath = scene.path,
                Name = scene.name
            };

            return sceneIdentifier;
        }

        public static SceneIdentifier GetSceneIdentifierPayload(SceneSafeRef sceneSafeRef)
        {
            var sceneIdentifier = new SceneIdentifier
            {
                Guid = sceneSafeRef.Identifier.Guid.ToString(),
                AssetBundlePath = sceneSafeRef.Identifier.Path.ToString(),
                Name = sceneSafeRef.Identifier.Name.ToString()
            };

            return sceneIdentifier;
        }

        public static string GetTypeUrl(IDescriptor descriptor, string prefix) => !prefix.EndsWith("/")
            ? prefix + "/" + descriptor.FullName
            : prefix + descriptor.FullName;

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

        /// <summary>
        /// Fallback for an enum value that has no counterpart in the record format, e.g. a value added by a newer
        /// Unity version or a corrupted field. Recording must not abort on data it cannot represent, so the value is
        /// recorded as the payload enum's UNSPECIFIED member and a warning is logged once per value and recording.
        /// </summary>
        internal static TPayload UnknownEnumValue<TValue, TPayload>(TValue value)
            where TValue : struct, Enum
            where TPayload : struct, Enum
        {
            // UNSPECIFIED is the zero member for the few enums designed with it, and -1 for the others, where it was
            // added without renumbering so that existing records keep their meaning.
            var unspecified = default(TPayload).ToString() == "Unspecified"
                ? default
                : (TPayload)Enum.ToObject(typeof(TPayload), -1);

            ErrorReporter.Warn($"{typeof(TValue).FullName}.{value}",
                $"Unknown {typeof(TValue).Name} value '{value}', recorded as unspecified.");
            return unspecified;
        }

        public static AmbientMode ToPayload(this UnityEngine.Rendering.AmbientMode ambientMode)
        {
            return ambientMode switch
            {
                UnityEngine.Rendering.AmbientMode.Custom => AmbientMode.Custom,
                UnityEngine.Rendering.AmbientMode.Skybox => AmbientMode.Skybox,
                UnityEngine.Rendering.AmbientMode.Trilight => AmbientMode.Trilight,
                UnityEngine.Rendering.AmbientMode.Flat => AmbientMode.Flat,
                _ => UnknownEnumValue<UnityEngine.Rendering.AmbientMode, AmbientMode>(ambientMode)
            };
        }

        public static FogMode ToPayload(this UnityEngine.FogMode fogMode)
        {
            return fogMode switch
            {
                UnityEngine.FogMode.Linear => FogMode.Linear,
                UnityEngine.FogMode.Exponential => FogMode.Exponential,
                UnityEngine.FogMode.ExponentialSquared => FogMode.ExponentialSquared,
                _ => UnknownEnumValue<UnityEngine.FogMode, FogMode>(fogMode)
            };
        }

        public static DefaultReflectionMode ToPayload(
            this UnityEngine.Rendering.DefaultReflectionMode defaultReflectionMode)
        {
            return defaultReflectionMode switch
            {
                UnityEngine.Rendering.DefaultReflectionMode.Skybox => DefaultReflectionMode.Skybox,
                UnityEngine.Rendering.DefaultReflectionMode.Custom => DefaultReflectionMode.Custom,
                _ => UnknownEnumValue<UnityEngine.Rendering.DefaultReflectionMode, DefaultReflectionMode>(defaultReflectionMode)
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
                _ => UnknownEnumValue<UnityEngine.LightType, LightType>(lightType)
            };
        }

        public static LightShape ToPayload(this UnityEngine.LightShape lightShape)
        {
            return lightShape switch
            {
                UnityEngine.LightShape.Cone => LightShape.Cone,
                UnityEngine.LightShape.Pyramid => LightShape.Pyramid,
                UnityEngine.LightShape.Box => LightShape.Box,
                _ => UnknownEnumValue<UnityEngine.LightShape, LightShape>(lightShape)
            };
        }

        public static LightShadows ToPayload(this UnityEngine.LightShadows lightShadows)
        {
            return lightShadows switch
            {
                UnityEngine.LightShadows.None => LightShadows.None,
                UnityEngine.LightShadows.Hard => LightShadows.Hard,
                UnityEngine.LightShadows.Soft => LightShadows.Soft,
                _ => UnknownEnumValue<UnityEngine.LightShadows, LightShadows>(lightShadows)
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
                _ => UnknownEnumValue<UnityEngine.Rendering.LightShadowResolution, LightShadowResolution>(lightShadowResolution)
            };
        }

        public static LightShadowCasterMode ToPayload(this UnityEngine.LightShadowCasterMode lightShadowCasterMode)
        {
            return lightShadowCasterMode switch
            {
                UnityEngine.LightShadowCasterMode.Default => LightShadowCasterMode.Default,
                UnityEngine.LightShadowCasterMode.NonLightmappedOnly => LightShadowCasterMode.NonLightmappedOnly,
                UnityEngine.LightShadowCasterMode.Everything => LightShadowCasterMode.Everything,
                _ => UnknownEnumValue<UnityEngine.LightShadowCasterMode, LightShadowCasterMode>(lightShadowCasterMode)
            };
        }

#if UNITY_6000_0_OR_NEWER
        public static LightUnit ToPayload(this UnityEngine.Rendering.LightUnit lightUnit)
        {
            return lightUnit switch
            {
                UnityEngine.Rendering.LightUnit.Lumen => LightUnit.Lumen,
                UnityEngine.Rendering.LightUnit.Candela => LightUnit.Candela,
                UnityEngine.Rendering.LightUnit.Lux => LightUnit.Lux,
                UnityEngine.Rendering.LightUnit.Nits => LightUnit.Nits,
                UnityEngine.Rendering.LightUnit.Ev100 => LightUnit.Ev100,
                _ => LightUnit.Lumen
            };
        }
#endif

        public static Bounds ToPayload(this UnityEngine.Bounds bounds)
        {
            return new Bounds
            {
                Center = bounds.center.ToPayload(),
                Size = bounds.size.ToPayload()
            };
        }

        public static LightmapData ToPayload(this UnityEngine.LightmapData lightmapData)
        {
            return new LightmapData
            {
                LightmapColorTexture = GetAssetIdentifierPayload(lightmapData.lightmapColor),
                LightmapDirTexture = GetAssetIdentifierPayload(lightmapData.lightmapDir),
                LightmapShadowMaskTexture = GetAssetIdentifierPayload(lightmapData.shadowMask)
            };
        }

        public static LightmapsMode ToPayload(this UnityEngine.LightmapsMode lightmapsMode)
        {
            return lightmapsMode switch
            {
                UnityEngine.LightmapsMode.NonDirectional => LightmapsMode.NonDirectional,
                UnityEngine.LightmapsMode.CombinedDirectional => LightmapsMode.CombinedDirectional,
                _ => UnknownEnumValue<UnityEngine.LightmapsMode, LightmapsMode>(lightmapsMode)
            };
        }

        public static ReflectionProbeMode ToPayload(this UnityEngine.Rendering.ReflectionProbeMode reflectionProbeMode)
        {
            return reflectionProbeMode switch
            {
                UnityEngine.Rendering.ReflectionProbeMode.Custom => ReflectionProbeMode.Custom,
                UnityEngine.Rendering.ReflectionProbeMode.Baked => ReflectionProbeMode.Baked,
                UnityEngine.Rendering.ReflectionProbeMode.Realtime => ReflectionProbeMode.Realtime,
                _ => UnknownEnumValue<UnityEngine.Rendering.ReflectionProbeMode, ReflectionProbeMode>(reflectionProbeMode)
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
                _ => UnknownEnumValue<UnityEngine.Rendering.ReflectionProbeRefreshMode, ReflectionProbeRefreshMode>(reflectionProbeRefreshMode)
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
                _ => UnknownEnumValue<UnityEngine.Rendering.ReflectionProbeTimeSlicingMode, ReflectionProbeTimeSlicingMode>(reflectionProbeTimeSlicingMode)
            };
        }

        public static ReflectionProbeClearFlags ToPayload(
            this UnityEngine.Rendering.ReflectionProbeClearFlags reflectionProbeClearFlags)
        {
            return reflectionProbeClearFlags switch
            {
                UnityEngine.Rendering.ReflectionProbeClearFlags.Skybox => ReflectionProbeClearFlags.Skybox,
                UnityEngine.Rendering.ReflectionProbeClearFlags.SolidColor => ReflectionProbeClearFlags.SolidColor,
                _ => UnknownEnumValue<UnityEngine.Rendering.ReflectionProbeClearFlags, ReflectionProbeClearFlags>(reflectionProbeClearFlags)
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
                _ => UnknownEnumValue<UnityEngine.Rendering.ShadowCastingMode, ShadowCastingMode>(shadowCastingMode)
            };
        }

        public static ReflectionProbeUsage ToPayload(
            this UnityEngine.Rendering.ReflectionProbeUsage reflectionProbeUsage)
        {
            return reflectionProbeUsage switch
            {
                UnityEngine.Rendering.ReflectionProbeUsage.Off => ReflectionProbeUsage.Off,
                UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes => ReflectionProbeUsage.BlendProbes,
                UnityEngine.Rendering.ReflectionProbeUsage.BlendProbesAndSkybox => ReflectionProbeUsage
                    .BlendProbesAndSkybox,
                UnityEngine.Rendering.ReflectionProbeUsage.Simple => ReflectionProbeUsage.Simple,
                _ => UnknownEnumValue<UnityEngine.Rendering.ReflectionProbeUsage, ReflectionProbeUsage>(reflectionProbeUsage)
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
                _ => UnknownEnumValue<UnityEngine.Rendering.OpaqueSortMode, OpaqueSortMode>(opaqueSortMode)
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
                _ => UnknownEnumValue<UnityEngine.TransparencySortMode, TransparencySortMode>(transparencySortMode)
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
                _ => UnknownEnumValue<UnityEngine.RenderingPath, RenderingPath>(renderingPath)
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
                _ => UnknownEnumValue<Camera.GateFitMode, CameraGateFitMode>(gateFitMode)
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
                _ => UnknownEnumValue<StereoTargetEyeMask, CameraStereoTargetEyeMask>(stereoTargetEyeMask)
            };
        }

        public static LoadSceneMode ToPayload(this UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
        {
            return loadSceneMode switch
            {
                UnityEngine.SceneManagement.LoadSceneMode.Additive => LoadSceneMode.Additive,
                UnityEngine.SceneManagement.LoadSceneMode.Single => LoadSceneMode.Single,
                _ => UnknownEnumValue<UnityEngine.SceneManagement.LoadSceneMode, LoadSceneMode>(loadSceneMode)
            };
        }

        public static WeightedMode ToPayload(this UnityEngine.WeightedMode weightedMode)
        {
            return weightedMode switch
            {
                UnityEngine.WeightedMode.None => WeightedMode.None,
                UnityEngine.WeightedMode.In => WeightedMode.In,
                UnityEngine.WeightedMode.Out => WeightedMode.Out,
                UnityEngine.WeightedMode.Both => WeightedMode.Both,
                _ => UnknownEnumValue<UnityEngine.WeightedMode, WeightedMode>(weightedMode)
            };
        }

        public static RenderMode ToPayload(this UnityEngine.RenderMode renderMode)
        {
            return renderMode switch
            {
                UnityEngine.RenderMode.ScreenSpaceOverlay => RenderMode.ScreenSpaceOverlay,
                UnityEngine.RenderMode.ScreenSpaceCamera => RenderMode.ScreenSpaceCamera,
                UnityEngine.RenderMode.WorldSpace => RenderMode.WorldSpace,
                _ => UnknownEnumValue<UnityEngine.RenderMode, RenderMode>(renderMode)
            };
        }

        public static StandaloneRenderResize ToPayload(this UnityEngine.StandaloneRenderResize renderResize)
        {
            return renderResize switch
            {
                UnityEngine.StandaloneRenderResize.Enabled => StandaloneRenderResize.Enabled,
                UnityEngine.StandaloneRenderResize.Disabled => StandaloneRenderResize.Disabled,
                _ => UnknownEnumValue<UnityEngine.StandaloneRenderResize, StandaloneRenderResize>(renderResize)
            };
        }

        public static ScaleMode ToPayload(this CanvasScaler.ScaleMode scaleMode)
        {
            return scaleMode switch
            {
                CanvasScaler.ScaleMode.ConstantPixelSize => ScaleMode.ConstantPixelSize,
                CanvasScaler.ScaleMode.ConstantPhysicalSize => ScaleMode.ConstantPhysicalSize,
                CanvasScaler.ScaleMode.ScaleWithScreenSize => ScaleMode.ScaleWithScreenSize,
                _ => UnknownEnumValue<CanvasScaler.ScaleMode, ScaleMode>(scaleMode)
            };
        }

        public static ScreenMatchMode ToPayload(this CanvasScaler.ScreenMatchMode screenMatchMode)
        {
            return screenMatchMode switch
            {
                CanvasScaler.ScreenMatchMode.MatchWidthOrHeight => ScreenMatchMode.MatchWidthOrHeight,
                CanvasScaler.ScreenMatchMode.Expand => ScreenMatchMode.Expand,
                CanvasScaler.ScreenMatchMode.Shrink => ScreenMatchMode.Shrink,
                _ => UnknownEnumValue<CanvasScaler.ScreenMatchMode, ScreenMatchMode>(screenMatchMode)
            };
        }

        public static Unit ToPayload(this CanvasScaler.Unit unit)
        {
            return unit switch
            {
                CanvasScaler.Unit.Centimeters => Unit.Centimeters,
                CanvasScaler.Unit.Millimeters => Unit.Millimeters,
                CanvasScaler.Unit.Inches => Unit.Inches,
                CanvasScaler.Unit.Points => Unit.Points,
                CanvasScaler.Unit.Picas => Unit.Picas,
                _ => UnknownEnumValue<CanvasScaler.Unit, Unit>(unit)
            };
        }

        public static FontStyle ToPayload(this UnityEngine.FontStyle fontStyle)
        {
            return fontStyle switch
            {
                UnityEngine.FontStyle.Normal => FontStyle.Normal,
                UnityEngine.FontStyle.Bold => FontStyle.Bold,
                UnityEngine.FontStyle.Italic => FontStyle.Italic,
                UnityEngine.FontStyle.BoldAndItalic => FontStyle.BoldAndItalic,
                _ => UnknownEnumValue<UnityEngine.FontStyle, FontStyle>(fontStyle)
            };
        }

        public static TextAnchor ToPayload(this UnityEngine.TextAnchor textAnchor)
        {
            return textAnchor switch
            {
                UnityEngine.TextAnchor.UpperLeft => TextAnchor.UpperLeft,
                UnityEngine.TextAnchor.UpperCenter => TextAnchor.UpperCenter,
                UnityEngine.TextAnchor.UpperRight => TextAnchor.UpperRight,
                UnityEngine.TextAnchor.MiddleLeft => TextAnchor.MiddleLeft,
                UnityEngine.TextAnchor.MiddleCenter => TextAnchor.MiddleCenter,
                UnityEngine.TextAnchor.MiddleRight => TextAnchor.MiddleRight,
                UnityEngine.TextAnchor.LowerLeft => TextAnchor.LowerLeft,
                UnityEngine.TextAnchor.LowerCenter => TextAnchor.LowerCenter,
                UnityEngine.TextAnchor.LowerRight => TextAnchor.LowerRight,
                _ => UnknownEnumValue<UnityEngine.TextAnchor, TextAnchor>(textAnchor)
            };
        }

        public static HorizontalWrapMode ToPayload(this UnityEngine.HorizontalWrapMode horizontalWrapMode)
        {
            return horizontalWrapMode switch
            {
                UnityEngine.HorizontalWrapMode.Wrap => HorizontalWrapMode.Wrap,
                UnityEngine.HorizontalWrapMode.Overflow => HorizontalWrapMode.Overflow,
                _ => UnknownEnumValue<UnityEngine.HorizontalWrapMode, HorizontalWrapMode>(horizontalWrapMode)
            };
        }

        public static VerticalWrapMode ToPayload(this UnityEngine.VerticalWrapMode verticalWrapMode)
        {
            return verticalWrapMode switch
            {
                UnityEngine.VerticalWrapMode.Truncate => VerticalWrapMode.Truncate,
                UnityEngine.VerticalWrapMode.Overflow => VerticalWrapMode.Overflow,
                _ => UnknownEnumValue<UnityEngine.VerticalWrapMode, VerticalWrapMode>(verticalWrapMode)
            };
        }

        public static ImageType ToPayload(this Image.Type imageType)
        {
            return imageType switch
            {
                Image.Type.Simple => ImageType.Simple,
                Image.Type.Sliced => ImageType.Sliced,
                Image.Type.Tiled => ImageType.Tiled,
                Image.Type.Filled => ImageType.Filled,
                _ => UnknownEnumValue<Image.Type, ImageType>(imageType)
            };
        }

        public static FitMode ToPayload(this ContentSizeFitter.FitMode fitMode)
        {
            return fitMode switch
            {
                ContentSizeFitter.FitMode.Unconstrained => FitMode.Unconstrained,
                ContentSizeFitter.FitMode.MinSize => FitMode.MinSize,
                ContentSizeFitter.FitMode.PreferredSize => FitMode.PrefSize,
                _ => UnknownEnumValue<ContentSizeFitter.FitMode, FitMode>(fitMode)
            };
        }

        public static SpeakerMode ToPayload(this AudioSpeakerMode audioSpeakerMode)
        {
            return audioSpeakerMode switch
            {
                AudioSpeakerMode.Mono => SpeakerMode.Mono,
                AudioSpeakerMode.Stereo => SpeakerMode.Stereo,
                AudioSpeakerMode.Quad => SpeakerMode.Quad,
                AudioSpeakerMode.Surround => SpeakerMode.Surround,
                AudioSpeakerMode.Mode5point1 => SpeakerMode.Surround5Point1,
                AudioSpeakerMode.Mode7point1 => SpeakerMode.Surround7Point1,
                AudioSpeakerMode.Prologic => SpeakerMode.Prologic,
                _ => UnknownEnumValue<AudioSpeakerMode, SpeakerMode>(audioSpeakerMode)
            };
        }

        public static ColorSpace ToPayload(this UnityEngine.ColorSpace colorSpace)
        {
            return colorSpace switch
            {
                UnityEngine.ColorSpace.Uninitialized => ColorSpace.Uninitialized,
                UnityEngine.ColorSpace.Gamma => ColorSpace.Gamma,
                UnityEngine.ColorSpace.Linear => ColorSpace.Linear,
                _ => UnknownEnumValue<UnityEngine.ColorSpace, ColorSpace>(colorSpace)
            };
        }

        public static ColorGradient.Types.GradientMode ToPayload(this GradientMode gradientMode)
        {
            return gradientMode switch
            {
                GradientMode.Blend => ColorGradient.Types.GradientMode.Blend,
                GradientMode.Fixed => ColorGradient.Types.GradientMode.Fixed,
                GradientMode.PerceptualBlend => ColorGradient.Types.GradientMode.PerceptualBlend,
                _ => UnknownEnumValue<GradientMode, ColorGradient.Types.GradientMode>(gradientMode)
            };
        }

        public static ColorGradient ToPayload(this Gradient gradient)
        {
            var colorGradient = new ColorGradient
            {
                ColorSpace = gradient.colorSpace.ToPayload(),
                Mode = gradient.mode.ToPayload()
            };

            foreach (var colorKey in gradient.colorKeys)
            {
                colorGradient.ColorKeys.Add(new ColorGradient.Types.ColorKey
                {
                    Color = colorKey.color.ToPayload(),
                    Time = colorKey.time
                });
            }

            foreach (var alphaKey in gradient.alphaKeys)
            {
                colorGradient.AlphaKeys.Add(new ColorGradient.Types.AlphaKey
                {
                    Alpha = alphaKey.alpha,
                    Time = alphaKey.time
                });
            }

            return colorGradient;
        }

        public static Alignment ToPayload(this LineAlignment alignment)
        {
            return alignment switch
            {
                LineAlignment.View => Alignment.View,
                LineAlignment.TransformZ => Alignment.TransformZ,
                _ => UnknownEnumValue<LineAlignment, Alignment>(alignment)
            };
        }

        public static TextureMode ToPayload(this LineTextureMode lineTextureMode)
        {
            return lineTextureMode switch
            {
                LineTextureMode.Stretch => TextureMode.Stretch,
                LineTextureMode.Tile => TextureMode.Tile,
                LineTextureMode.DistributePerSegment => TextureMode.DistributePerSegment,
                LineTextureMode.RepeatPerSegment => TextureMode.RepeatPerSegment,
                LineTextureMode.Static => TextureMode.Static,
                _ => UnknownEnumValue<LineTextureMode, TextureMode>(lineTextureMode)
            };
        }

        public static MaskInteraction ToPayload(this SpriteMaskInteraction spriteMaskInteraction)
        {
            return spriteMaskInteraction switch
            {
                SpriteMaskInteraction.None => MaskInteraction.None,
                SpriteMaskInteraction.VisibleInsideMask => MaskInteraction.VisibleInside,
                SpriteMaskInteraction.VisibleOutsideMask => MaskInteraction.VisibleOutside,
                _ => UnknownEnumValue<SpriteMaskInteraction, MaskInteraction>(spriteMaskInteraction)
            };
        }

        public static AnimationCurve ToPayload(this UnityEngine.AnimationCurve animationCurve)
        {
            var animationCurveSample = new AnimationCurve();

            foreach (var key in animationCurve.keys)
            {
                animationCurveSample.Keyframes.Add(new AnimationCurveKeyFrame
                {
                    Time = key.time,
                    Value = key.value,
                    InTangent = key.inTangent,
                    OutTangent = key.outTangent,
                    InWeight = key.inWeight,
                    OutWeight = key.outWeight,
                    WeightedMode = key.weightedMode.ToPayload()
                });
            }

            return animationCurveSample;
        }

#if URP_ENABLED
        public static CameraOverrideOption ToPayload(
            this UnityEngine.Rendering.Universal.CameraOverrideOption cameraOverrideOption)
        {
            return cameraOverrideOption switch
            {
                UnityEngine.Rendering.Universal.CameraOverrideOption.Off => CameraOverrideOption.Off,
                UnityEngine.Rendering.Universal.CameraOverrideOption.On => CameraOverrideOption.On,
                UnityEngine.Rendering.Universal.CameraOverrideOption.UsePipelineSettings => CameraOverrideOption
                    .UsePipelineSettings,
                _ => UnknownEnumValue<UnityEngine.Rendering.Universal.CameraOverrideOption, CameraOverrideOption>(cameraOverrideOption)
            };
        }

        public static CameraRenderType ToPayload(this UnityEngine.Rendering.Universal.CameraRenderType cameraRenderType)
        {
            return cameraRenderType switch
            {
                UnityEngine.Rendering.Universal.CameraRenderType.Base => CameraRenderType.Base,
                UnityEngine.Rendering.Universal.CameraRenderType.Overlay => CameraRenderType.Overlay,
                _ => UnknownEnumValue<UnityEngine.Rendering.Universal.CameraRenderType, CameraRenderType>(cameraRenderType)
            };
        }

        public static AntialiasingMode ToPayload(this UnityEngine.Rendering.Universal.AntialiasingMode antialiasingMode)
        {
            return antialiasingMode switch
            {
                UnityEngine.Rendering.Universal.AntialiasingMode.None => AntialiasingMode.None,
                UnityEngine.Rendering.Universal.AntialiasingMode.FastApproximateAntialiasing => AntialiasingMode
                    .FastApproximateAntialiasing,
                UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing => AntialiasingMode
                    .SubpixelMorphologicalAntiAliasing,
                _ => UnknownEnumValue<UnityEngine.Rendering.Universal.AntialiasingMode, AntialiasingMode>(antialiasingMode)
            };
        }

        public static AntialiasingQuality ToPayload(
            this UnityEngine.Rendering.Universal.AntialiasingQuality antialiasingQuality)
        {
            return antialiasingQuality switch
            {
                UnityEngine.Rendering.Universal.AntialiasingQuality.Low => AntialiasingQuality.Low,
                UnityEngine.Rendering.Universal.AntialiasingQuality.Medium => AntialiasingQuality.Medium,
                UnityEngine.Rendering.Universal.AntialiasingQuality.High => AntialiasingQuality.High,
                _ => UnknownEnumValue<UnityEngine.Rendering.Universal.AntialiasingQuality, AntialiasingQuality>(antialiasingQuality)
            };
        }
#endif
    }
}