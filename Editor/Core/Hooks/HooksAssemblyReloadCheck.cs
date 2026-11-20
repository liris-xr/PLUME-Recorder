using System;
using System.IO;
using System.Text;
using PLUME.Core.Hooks;
using UnityEditor;
using Logger = PLUME.Core.Logger;

namespace PLUME.Editor.Core.Hooks
{
    /// <summary>
    /// Hooked assemblies reference PLUME.Recorder.Hooks without Unity knowing it, so Mono resolves the hooks assembly
    /// before Unity reloads it: once hooks were injected, a change to the hooks assembly is not picked up until the
    /// editor restarts. This check tells the user instead of silently running stale hooks.
    /// </summary>
    internal static class HooksAssemblyReloadCheck
    {
        private const string HooksAssemblyPath = "Library/ScriptAssemblies/PLUME.Recorder.Hooks.dll";

        [InitializeOnLoadMethod]
        private static void CheckHooksAssemblyIsUpToDate()
        {
            if (!File.Exists(HooksAssemblyPath))
                return;

            var loadedVersion = typeof(HooksRegistry).Assembly.ManifestModule.ModuleVersionId;
            Guid compiledVersion;
            try
            {
                compiledVersion = ReadModuleVersionId(File.ReadAllBytes(HooksAssemblyPath));
            }
            catch (Exception)
            {
                // Not being able to tell must never get in the way: the check is only a convenience.
                return;
            }

            if (compiledVersion != Guid.Empty && compiledVersion != loadedVersion)
            {
                Logger.LogWarning(nameof(PLUME),
                    "PLUME hooks were modified but Unity is still running their previous version. " +
                    "Restart Unity to apply the changes.");
            }
        }

        /// <summary>
        /// Reads the module version id (MVID) of a compiled assembly: the first entry of the #GUID metadata heap.
        /// </summary>
        private static Guid ReadModuleVersionId(byte[] assembly)
        {
            var metadataRoot = IndexOf(assembly, Encoding.ASCII.GetBytes("BSJB"));
            var versionLength = BitConverter.ToInt32(assembly, metadataRoot + 12);
            var offset = metadataRoot + 16 + versionLength;
            var streamCount = BitConverter.ToUInt16(assembly, offset + 2);
            offset += 4;

            for (var i = 0; i < streamCount; i++)
            {
                var streamOffset = BitConverter.ToInt32(assembly, offset);
                offset += 8;

                var nameStart = offset;
                while (assembly[offset] != 0)
                    offset++;
                var name = Encoding.ASCII.GetString(assembly, nameStart, offset - nameStart);
                // Stream names are null-terminated and padded to 4 bytes.
                offset = nameStart + ((offset - nameStart + 4) & ~3);

                if (name == "#GUID")
                    return new Guid(new ReadOnlySpan<byte>(assembly, metadataRoot + streamOffset, 16));
            }

            return Guid.Empty;
        }

        private static int IndexOf(byte[] data, byte[] pattern)
        {
            for (var i = 0; i <= data.Length - pattern.Length; i++)
            {
                var match = true;
                for (var j = 0; j < pattern.Length && match; j++)
                    match = data[i + j] == pattern[j];

                if (match)
                    return i;
            }

            throw new BadImageFormatException("No metadata root found.");
        }
    }
}
