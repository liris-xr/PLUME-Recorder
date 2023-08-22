using System;
using PLUME.Sample.Common;
using PLUME.Sample.Unity;
using UnityEngine;
using Bounds = PLUME.Sample.Common.Bounds;
using Color = PLUME.Sample.Common.Color;
using FogMode = PLUME.Sample.Unity.FogMode;
using LightShadowCasterMode = PLUME.Sample.Unity.LightShadowCasterMode;
using LightShadows = PLUME.Sample.Unity.LightShadows;
using LightShape = PLUME.Sample.Unity.LightShape;
using LightType = PLUME.Sample.Unity.LightType;
using Matrix4x4 = PLUME.Sample.Common.Matrix4x4;
using Object = UnityEngine.Object;
using Quaternion = PLUME.Sample.Common.Quaternion;
using Vector2 = PLUME.Sample.Common.Vector2;
using Vector3 = PLUME.Sample.Common.Vector3;
using Vector4 = PLUME.Sample.Common.Vector4;

namespace PLUME
{
    public static class PayloadExtensions
    {
        public static AssetIdentifier ToAssetIdentifierPayload(this Object obj)
        {
            return new AssetIdentifier
            {
                Id = obj.GetHashCode().ToString(),
                Hash = obj.GetRecorderHash(),
            };
        }

        public static ComponentIdentifier ToIdentifierPayload(this Component component)
        {
            return new ComponentIdentifier
            {
                Id = component.GetHashCode().ToString(),
                ParentId = new TransformGameObjectIdentifier
                {
                    TransformId = component.transform.GetHashCode().ToString(),
                    GameObjectId = component.gameObject.GetHashCode().ToString()
                }
            };
        }

        public static TransformGameObjectIdentifier ToIdentifierPayload(this GameObject go)
        {
            return new TransformGameObjectIdentifier
            {
                // We use GetHashCode because it returns m_InstanceID directly without checking if on main thread which is slow
                // TODO: If GetHashCode is overriden, this might cause issue. Find a better way for this?
                TransformId = go.transform.GetHashCode().ToString(),
                GameObjectId = go.GetHashCode().ToString()
            };
        }

        public static TransformGameObjectIdentifier ToIdentifierPayload(this Transform t)
        {
            return new TransformGameObjectIdentifier
            {
                TransformId = t.GetHashCode().ToString(),
                GameObjectId = t.gameObject.GetHashCode().ToString()
            };
        }
        
        public static Vector2 ToPayload(this UnityEngine.Vector2 vec)
        {
            return new Vector2
            {
                X = vec.x,
                Y = vec.y,
            };
        }

        public static UnityEngine.Vector2 ToEngineType(this Vector2 vec)
        {
            return new UnityEngine.Vector2
            {
                x = vec.X,
                y = vec.Y,
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

        public static UnityEngine.Vector3 ToEngineType(this Vector3 vec)
        {
            return new UnityEngine.Vector3
            {
                x = vec.X,
                y = vec.Y,
                z = vec.Z
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

        public static UnityEngine.Vector4 ToEngineType(this Vector4 vec)
        {
            return new UnityEngine.Vector4
            {
                x = vec.X,
                y = vec.Y,
                z = vec.Z,
                w = vec.W
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

        public static UnityEngine.Quaternion ToEngineType(this Quaternion vec)
        {
            return new UnityEngine.Quaternion
            {
                x = vec.X,
                y = vec.Y,
                z = vec.Z,
                w = vec.W
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

        public static UnityEngine.Color ToEngineType(this Color color)
        {
            return new UnityEngine.Color
            {
                r = color.R,
                g = color.G,
                b = color.B,
                a = color.A
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

        public static UnityEngine.Rendering.SphericalHarmonicsL2 ToEngineType(
            this SphericalHarmonicsL2 sphericalHarmonicsL2)
        {
            var shl2 = new UnityEngine.Rendering.SphericalHarmonicsL2
            {
                [0, 0] = sphericalHarmonicsL2.Shr0,
                [0, 1] = sphericalHarmonicsL2.Shr1,
                [0, 2] = sphericalHarmonicsL2.Shr2,
                [0, 3] = sphericalHarmonicsL2.Shr3,
                [0, 4] = sphericalHarmonicsL2.Shr4,
                [0, 5] = sphericalHarmonicsL2.Shr5,
                [0, 6] = sphericalHarmonicsL2.Shr6,
                [0, 7] = sphericalHarmonicsL2.Shr7,
                [0, 8] = sphericalHarmonicsL2.Shr8,
                [0, 9] = sphericalHarmonicsL2.Shg0,
                [0, 10] = sphericalHarmonicsL2.Shg1,
                [0, 11] = sphericalHarmonicsL2.Shg2,
                [0, 12] = sphericalHarmonicsL2.Shg3,
                [0, 13] = sphericalHarmonicsL2.Shg4,
                [0, 14] = sphericalHarmonicsL2.Shg5,
                [0, 15] = sphericalHarmonicsL2.Shg6,
                [0, 16] = sphericalHarmonicsL2.Shg7,
                [0, 17] = sphericalHarmonicsL2.Shg8,
                [0, 18] = sphericalHarmonicsL2.Shb0,
                [0, 19] = sphericalHarmonicsL2.Shb1,
                [0, 20] = sphericalHarmonicsL2.Shb2,
                [0, 21] = sphericalHarmonicsL2.Shb3,
                [0, 22] = sphericalHarmonicsL2.Shb4,
                [0, 23] = sphericalHarmonicsL2.Shb5,
                [0, 24] = sphericalHarmonicsL2.Shb6,
                [0, 25] = sphericalHarmonicsL2.Shb7,
                [0, 26] = sphericalHarmonicsL2.Shb8
            };
            return shl2;
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

        public static UnityEngine.LightType ToEngineType(this LightType lightType)
        {
            return lightType switch
            {
                LightType.Point => UnityEngine.LightType.Point,
                LightType.Directional => UnityEngine.LightType.Directional,
                LightType.Spot => UnityEngine.LightType.Spot,
                LightType.Rectangle => UnityEngine.LightType.Rectangle,
                LightType.Disc => UnityEngine.LightType.Disc,
                LightType.Area => UnityEngine.LightType.Area,
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

        public static UnityEngine.LightShape ToEngineType(this LightShape lightShape)
        {
            return lightShape switch
            {
                LightShape.Cone => UnityEngine.LightShape.Cone,
                LightShape.Pyramid => UnityEngine.LightShape.Pyramid,
                LightShape.Box => UnityEngine.LightShape.Box,
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

        public static UnityEngine.LightShadows ToEngineType(this LightShadows lightShadows)
        {
            return lightShadows switch
            {
                LightShadows.None => UnityEngine.LightShadows.None,
                LightShadows.Hard => UnityEngine.LightShadows.Hard,
                LightShadows.Soft => UnityEngine.LightShadows.Soft,
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

        public static UnityEngine.Rendering.LightShadowResolution ToEngineType(
            this LightShadowResolution lightShadowResolution)
        {
            return lightShadowResolution switch
            {
                LightShadowResolution.FromQualitySettings => UnityEngine.Rendering.LightShadowResolution
                    .FromQualitySettings,
                LightShadowResolution.Low => UnityEngine.Rendering.LightShadowResolution.Low,
                LightShadowResolution.Medium => UnityEngine.Rendering.LightShadowResolution.Medium,
                LightShadowResolution.High => UnityEngine.Rendering.LightShadowResolution.High,
                LightShadowResolution.VeryHigh => UnityEngine.Rendering.LightShadowResolution.VeryHigh,
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

        public static UnityEngine.LightShadowCasterMode ToEngineType(this LightShadowCasterMode lightShadowCasterMode)
        {
            return lightShadowCasterMode switch
            {
                LightShadowCasterMode.Default => UnityEngine.LightShadowCasterMode.Default,
                LightShadowCasterMode.NonLightmappedOnly => UnityEngine.LightShadowCasterMode.NonLightmappedOnly,
                LightShadowCasterMode.Everything => UnityEngine.LightShadowCasterMode.Everything,
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

        public static UnityEngine.FogMode ToEngineType(this FogMode fogMode)
        {
            return fogMode switch
            {
                FogMode.Linear => UnityEngine.FogMode.Linear,
                FogMode.Exponential => UnityEngine.FogMode.Exponential,
                FogMode.ExponentialSquared => UnityEngine.FogMode.ExponentialSquared,
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

        public static UnityEngine.Rendering.DefaultReflectionMode ToEngineType(
            this DefaultReflectionMode defaultReflectionMode)
        {
            return defaultReflectionMode switch
            {
                DefaultReflectionMode.Skybox => UnityEngine.Rendering.DefaultReflectionMode.Skybox,
                DefaultReflectionMode.Custom => UnityEngine.Rendering.DefaultReflectionMode.Custom,
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

        public static UnityEngine.Rendering.AmbientMode ToEngineType(this AmbientMode ambientMode)
        {
            return ambientMode switch
            {
                AmbientMode.Skybox => UnityEngine.Rendering.AmbientMode.Skybox,
                AmbientMode.Trilight => UnityEngine.Rendering.AmbientMode.Trilight,
                AmbientMode.Flat => UnityEngine.Rendering.AmbientMode.Flat,
                AmbientMode.Custom => UnityEngine.Rendering.AmbientMode.Custom,
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

        public static UnityEngine.Bounds ToEngineType(this Bounds bounds)
        {
            return new UnityEngine.Bounds
            {
                center = bounds.Center.ToEngineType(),
                extents = bounds.Extents.ToEngineType()
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

        public static UnityEngine.Matrix4x4 ToEngineType(this Matrix4x4 mtx)
        {
            return new UnityEngine.Matrix4x4
            {
                m00 = mtx.M00,
                m10 = mtx.M10,
                m20 = mtx.M20,
                m30 = mtx.M30,
                m01 = mtx.M01,
                m11 = mtx.M11,
                m21 = mtx.M21,
                m31 = mtx.M31,
                m02 = mtx.M02,
                m12 = mtx.M12,
                m22 = mtx.M22,
                m32 = mtx.M32,
                m03 = mtx.M03,
                m13 = mtx.M13,
                m23 = mtx.M23,
                m33 = mtx.M33
            };
        }
    }
}