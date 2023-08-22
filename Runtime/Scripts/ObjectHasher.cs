using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace PLUME
{
    public class ObjectHasher
    {
        private static readonly MethodInfo GetFirstPropertyNameIdByAttributeInfo =
            typeof(Material).GetMethod("GetFirstPropertyNameIdByAttribute",
                BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly int MainColorPropertyId = Shader.PropertyToID("_Color");
        private static readonly int MainTexturePropertyId = Shader.PropertyToID("_MainTex");

        private readonly Dictionary<Object, int> _cachedObjectHashes = new();

        public int Hash(Object obj)
        {
            if (obj == null) return 0;

            if (_cachedObjectHashes.ContainsKey(obj)) return _cachedObjectHashes[obj];

            int hash;

            switch (obj)
            {
                case Mesh mesh:
                {
                    // Make the hash invariant to instance/shared mesh name
                    var name = RemoveSuffix(obj.name, " Instance");
                    var vertexCount = mesh.vertexCount;
                    var bounds = mesh.bounds;
                    hash = CombineHashCodes(name.GetHashCode(), vertexCount.GetHashCode(), bounds.GetHashCode());
                    break;
                }
                case Material mat:
                {
                    // Make the hash invariant to instance/shared material name
                    var name = RemoveSuffix(obj.name, " (Instance)");
                    var shaderName = mat.shader.name;
                    var color = GetMaterialMainColor(mat);
                    var mainTexture = GetMaterialMainTexture(mat);
                    hash = CombineHashCodes(name.GetHashCode(), shaderName.GetHashCode(), color.GetHashCode(), Hash(mainTexture));
                    break;
                }
                case Texture tex:
                {
                    hash = CombineHashCodes(tex.name.GetHashCode(), tex.width.GetHashCode(), tex.height.GetHashCode());
                    break;
                }
                // TODO: Flare hash
                default:
                    hash = 0;
                    break;
            }

            _cachedObjectHashes[obj] = hash;
            return hash;
        }

        private static string RemoveSuffix(string val, string suffix)
        {
            var suffixIndex = val.LastIndexOf(suffix, StringComparison.InvariantCulture);
            return suffixIndex != -1 ? val.Remove(suffixIndex) : val;
        }

        private static Color? GetMaterialMainColor(Material mat)
        {
            var nameIdByAttribute =
                (int)GetFirstPropertyNameIdByAttributeInfo.Invoke(mat, new object[] { ShaderPropertyFlags.MainColor });

            if (nameIdByAttribute >= 0)
                return mat.GetColor(nameIdByAttribute);

            return mat.HasProperty(MainColorPropertyId) ? mat.GetColor(MainColorPropertyId) : null;
        }

        private static Texture GetMaterialMainTexture(Material mat)
        {
            var nameIdByAttribute =
                (int)GetFirstPropertyNameIdByAttributeInfo.Invoke(mat,
                    new object[] { ShaderPropertyFlags.MainTexture });

            if (nameIdByAttribute >= 0)
                return mat.GetTexture(nameIdByAttribute);

            return mat.HasProperty(MainTexturePropertyId) ? mat.GetTexture(MainTexturePropertyId) : null;
        }

        // https://referencesource.microsoft.com/#mscorlib/system/string.cs,0a17bbac4851d0d4
        private static int CombineHashCodes(params int[] hashCodes)
        {
            var hash1 = (5381 << 16) + 5381;
            var hash2 = hash1;

            var i = 0;
            foreach (var hashCode in hashCodes)
            {
                if (i % 2 == 0)
                    hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ hashCode;
                else
                    hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ hashCode;

                ++i;
            }

            return hash1 + hash2 * 1566083941;
        }
    }
}