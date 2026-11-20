using System.IO;
using System.Linq;
using PLUME.Core.Settings;
using PLUME.Editor.Core.Settings;
using UnityEditor;
using UnityEditor.Compilation;

namespace PLUME.Editor.Core.Hooks
{
    /// <summary>
    /// The hooks IL post-processor reads the assembly blacklist from the hooks settings at compile time, but the
    /// build cache does not track that file: when the blacklist changes, assemblies that did not change keep their
    /// previous injection. Every assembly is then rebuilt so that the new blacklist applies everywhere.
    /// </summary>
    internal sealed class HooksBlacklistWatcher : AssetPostprocessor
    {
        // Persisted in Library so that a change made while the editor was closed (e.g. a git pull) is applied too.
        private const string AppliedBlacklistPath = "Library/PLUME/AppliedHooksBlacklist.txt";

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            // Settings must not be loaded while Unity is still importing assets.
            EditorApplication.delayCall += RebuildIfBlacklistChanged;
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (importedAssets.Concat(deletedAssets).Any(path => path.EndsWith("/HooksSettings.asset")))
                RebuildIfBlacklistChanged();
        }

        private static void RebuildIfBlacklistChanged()
        {
            var settings = SettingsEditor.SettingsProvider.GetOrCreate<HooksSettings>();
            var blacklist = string.Join("\n", settings.BlacklistedAssemblyNames.OrderBy(name => name));
            var appliedBlacklist = File.Exists(AppliedBlacklistPath) ? File.ReadAllText(AppliedBlacklistPath) : null;

            if (blacklist == appliedBlacklist)
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(AppliedBlacklistPath)!);
            File.WriteAllText(AppliedBlacklistPath, blacklist);

            // The first run only records the blacklist the current assemblies were built with.
            if (appliedBlacklist != null)
                CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
        }
    }
}
