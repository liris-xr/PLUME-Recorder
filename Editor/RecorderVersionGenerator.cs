using System;
using System.IO;
using System.Linq;
using PLUME.Core.Recorder;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using Logger = PLUME.Core.Logger;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace PLUME.Editor
{
    /// <summary>
    /// Keeps <see cref="PlumeRecorder.Version"/> in sync with the "version" field of the package's package.json by
    /// generating Runtime/Core/Recorder/RecorderVersion.g.cs. The generated file is committed so that consumers of the
    /// package (including built players) get the correct version without running this generator.
    /// </summary>
    [InitializeOnLoad]
    public class RecorderVersionGenerator : AssetPostprocessor, IPreprocessBuildWithReport
    {
        private const string GeneratedFileRelativePath = "Runtime/Core/Recorder/RecorderVersion.g.cs";
        private const string BaseName = "PLUME Recorder";

        private readonly struct ParsedVersion
        {
            public readonly string Name;
            public readonly string Major;
            public readonly string Minor;
            public readonly string Patch;

            public ParsedVersion(string name, string major, string minor, string patch)
            {
                Name = name;
                Major = major;
                Minor = minor;
                Patch = patch;
            }

            public override string ToString() => $"{Name} {Major}.{Minor}.{Patch}";
        }

        public int callbackOrder => 0;

        static RecorderVersionGenerator()
        {
            // Delay to let the package manager finish initializing.
            EditorApplication.delayCall += Generate;
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            var packageInfo = GetPackageInfo();
            if (packageInfo == null) return;

            var packageJsonPath = $"{packageInfo.assetPath}/package.json";
            if (importedAssets.Any(p => string.Equals(p, packageJsonPath, StringComparison.OrdinalIgnoreCase)))
            {
                // Refreshing/importing from within a postprocessor is unsupported, defer until the import is done.
                EditorApplication.delayCall += Generate;
            }
        }

        /// <summary>
        /// Checks that the compiled version matches package.json (e.g. package.json was bumped but the generated file
        /// was not regenerated/recompiled yet). Generation cannot happen mid-build since it requires a recompilation.
        /// </summary>
        public void OnPreprocessBuild(BuildReport report)
        {
            var packageInfo = GetPackageInfo();
            if (packageInfo == null) return;

            var isEditable = IsEditable(packageInfo);

            if (!TryParseVersion(packageInfo.version, out _, out var error))
            {
                if (isEditable)
                    throw new BuildFailedException(error);
                Logger.LogWarning(error);
                return;
            }

            // Compare the raw version string (plain const) rather than the protobuf Version object, which would
            // require the editor assembly to reference Google.Protobuf.
            if (PlumeRecorder.PackageVersion == packageInfo.version)
                return;

            var message = $"PLUME recorder version mismatch: compiled '{PlumeRecorder.PackageVersion}' but " +
                          $"package.json is '{packageInfo.version}'.";

            // Only fail builds where the package is editable (package development). Consumers installing from
            // git/registry cannot regenerate the file, so don't break their builds.
            if (isEditable)
                throw new BuildFailedException(
                    $"{message} Open the project in the editor (or run 'PLUME/Regenerate Recorder Version') to " +
                    $"regenerate '{GeneratedFileRelativePath}', commit it, and build again. In batch-mode/CI builds, " +
                    "the regenerated file must already be committed.");

            Logger.LogWarning(message);
        }

        [MenuItem("PLUME/Regenerate Recorder Version")]
        private static void Generate()
        {
            var packageInfo = GetPackageInfo();
            if (packageInfo == null) return;

            // Packages installed from git/registry are immutable: the committed generated file is used as-is.
            if (!IsEditable(packageInfo))
                return;

            if (!TryParseVersion(packageInfo.version, out var version, out var error))
            {
                Logger.Log(UnityEngine.LogType.Error, error);
                return;
            }

            var content = GenerateSource(version, packageInfo.version);
            var filePath = Path.Combine(packageInfo.resolvedPath, GeneratedFileRelativePath);

            // Only write when changed to avoid triggering a recompilation loop. Line endings are normalized since
            // git may check the file out with CRLF.
            if (File.Exists(filePath) && NormalizeLineEndings(File.ReadAllText(filePath)) == content)
                return;

            File.WriteAllText(filePath, content);
            Logger.Log($"Generated recorder version {version} from package.json.");
            AssetDatabase.ImportAsset($"{packageInfo.assetPath}/{GeneratedFileRelativePath}");
        }

        private static PackageInfo GetPackageInfo()
        {
            return PackageInfo.FindForAssembly(typeof(PlumeRecorder).Assembly);
        }

        private static bool IsEditable(PackageInfo packageInfo)
        {
            return packageInfo.source == PackageSource.Embedded || packageInfo.source == PackageSource.Local;
        }

        private static string NormalizeLineEndings(string s)
        {
            return s.Replace("\r\n", "\n");
        }

        private static bool TryParseVersion(string semver, out ParsedVersion version, out string error)
        {
            version = default;
            error = null;

            // Strip build metadata (e.g. "+sha.abc"), then split off the prerelease (e.g. "-beta", "-rc.1").
            var plusIdx = semver.IndexOf('+');
            var withoutBuild = plusIdx >= 0 ? semver.Substring(0, plusIdx) : semver;

            var dashIdx = withoutBuild.IndexOf('-');
            var core = dashIdx >= 0 ? withoutBuild.Substring(0, dashIdx) : withoutBuild;
            var prerelease = dashIdx >= 0 ? withoutBuild.Substring(dashIdx + 1) : string.Empty;

            var parts = core.Split('.');
            if (parts.Length != 3 || parts.Any(p => !int.TryParse(p, out _)))
            {
                error = $"Invalid PLUME package version '{semver}' in package.json, expected " +
                        "MAJOR.MINOR.PATCH[-prerelease] (e.g. 4.0.0 or 4.0.0-beta).";
                return false;
            }

            // Prerelease is appended to the name so prereleases stay distinguishable from stable releases,
            // e.g. "-beta" -> "PLUME Recorder (Beta)", "-rc.1" -> "PLUME Recorder (Rc.1)".
            var name = prerelease.Length == 0
                ? BaseName
                : $"{BaseName} ({char.ToUpperInvariant(prerelease[0])}{prerelease.Substring(1)})";

            version = new ParsedVersion(name, parts[0], parts[1], parts[2]);
            return true;
        }

        private static string GenerateSource(ParsedVersion v, string rawVersion)
        {
            return
                "// <auto-generated>\n" +
                "// Generated by PLUME.Editor.RecorderVersionGenerator from package.json. Do not edit manually.\n" +
                "// </auto-generated>\n" +
                "using PLUME.Sample;\n" +
                "\n" +
                "namespace PLUME.Core.Recorder\n" +
                "{\n" +
                "    public sealed partial class PlumeRecorder\n" +
                "    {\n" +
                $"        public const string PackageVersion = \"{rawVersion}\";\n" +
                "\n" +
                "        public static readonly RecorderVersion Version = new()\n" +
                "        {\n" +
                $"            Name = \"{v.Name}\",\n" +
                $"            Major = \"{v.Major}\",\n" +
                $"            Minor = \"{v.Minor}\",\n" +
                $"            Patch = \"{v.Patch}\",\n" +
                "        };\n" +
                "    }\n" +
                "}\n";
        }
    }
}
