using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;
using UnityRuntimeGuid;

namespace PLUME.Core.Object
{
    /// <summary>
    /// Resolves the GUID recorded for a scene, making it unique per scene asset.
    /// <para>
    /// The scene GUID comes from the <see cref="SceneGuidRegistry"/> serialized in the scene. Duplicating a scene
    /// asset in the editor copies that registry, so every copy carries the same GUID: a record of a study loading
    /// several duplicated scenes one after the other then gives them all one identity, and a player cannot tell
    /// that the scene changed.
    /// </para>
    /// <para>
    /// The first scene seen with a registry GUID keeps it, so records of projects without duplicates are unchanged.
    /// Any other scene (different path, or name for scenes without a path) seen with the same registry GUID gets a
    /// GUID derived from the registry GUID and its path. The derivation is deterministic, so a scene unloaded and
    /// loaded again keeps the same GUID within the session.
    /// </para>
    /// <para>
    /// Object GUIDs come from the same copied registry and still collide between duplicated scenes. Only scene
    /// identity is disambiguated here; regenerating the registry of duplicated scenes fixes both in the project.
    /// </para>
    /// </summary>
    public static class SceneGuidResolver
    {
        private static readonly object Lock = new();

        // Registry GUID -> key (path, or name when the scene has no path) of the first scene seen with it.
        private static readonly Dictionary<string, string> FirstSceneKeyByRegistryGuid = new();

        // (Registry GUID, scene key) -> derived GUID, for every scene that is not the first one seen with its GUID.
        private static readonly Dictionary<(string, string), string> DerivedGuids = new();

        /// <summary>Key identifying the scene asset: its path, or its name for scenes without a path (e.g.
        /// DontDestroyOnLoad or scenes created at runtime).</summary>
        public static string GetSceneKey(Scene scene) => string.IsNullOrEmpty(scene.path) ? scene.name : scene.path;

        /// <summary>Returns the GUID to record for <paramref name="scene"/>, in the "N" format of the registry.</summary>
        public static string GetSceneGuid(Scene scene)
        {
            var registryGuid = SceneGuidRegistry.GetOrCreate(scene).SceneGuid;
            return Resolve(registryGuid, GetSceneKey(scene));
        }

        public static string Resolve(string registryGuid, string sceneKey)
        {
            lock (Lock)
            {
                if (!FirstSceneKeyByRegistryGuid.TryGetValue(registryGuid, out var firstSceneKey))
                {
                    FirstSceneKeyByRegistryGuid[registryGuid] = sceneKey;
                    return registryGuid;
                }

                if (firstSceneKey == sceneKey)
                    return registryGuid;

                if (DerivedGuids.TryGetValue((registryGuid, sceneKey), out var derivedGuid))
                    return derivedGuid;

                derivedGuid = DeriveGuid(registryGuid, sceneKey);
                DerivedGuids[(registryGuid, sceneKey)] = derivedGuid;
                return derivedGuid;
            }
        }

        // Name-based (MD5) GUID of "registryGuid|sceneKey", formatted like Guid.ToString("N").
        private static string DeriveGuid(string registryGuid, string sceneKey)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(registryGuid + "|" + sceneKey));

            // RFC 4122 version 3 (name-based, MD5) and variant bits.
            hash[6] = (byte)((hash[6] & 0x0F) | 0x30);
            hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

            var builder = new StringBuilder(32);
            foreach (var b in hash)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}
