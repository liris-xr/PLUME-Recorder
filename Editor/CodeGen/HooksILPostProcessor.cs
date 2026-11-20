using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PLUME.Core.Hooks;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace PLUME.Editor.CodeGen
{
    /// <summary>
    /// Injects the recorder hooks into compiled assemblies: calls to hooked Unity methods (Instantiate, Destroy,
    /// AddComponent, property setters...) are replaced by calls to the matching <c>*AndNotify</c> hook methods.
    /// Running as an IL post-processor (rather than rewriting Library/ScriptAssemblies after compilation) means the
    /// injected assemblies are build-cache artifacts: they never clash with DLLs already loaded by Unity, they are
    /// rebuilt when the recorder or its hooks change or are removed, and player builds are injected too.
    /// </summary>
    internal sealed class HooksILPostProcessor : ILPostProcessor
    {
        private const string HooksSettingsAssetPath = "Assets/Resources/Settings/PLUME/HooksSettings.asset";

        private static readonly HashSet<string> BaseBlacklistedAssemblyNames = new()
        {
            "PLUME.Recorder",
            "Unity.PLUME.Recorder.CodeGen",
            "UnityRuntimeGuid",
            "UnityRuntimeGuid.Editor",
            "fr.liris.unity-runtime-guid",
            "fr.liris.unity-runtime-guid.editor",
            "ProtoBurst",
            "UniTask",
            "UniTask.Addressables",
            "UniTask.DOTween",
            "UniTask.Linq",
            "UniTask.TextMeshPro",
            "UnityEngine",
            "UnityEngine.UI"
        };

        private static readonly Lazy<IReadOnlyList<Hook>> Hooks = new(DiscoverHooks);

        public override ILPostProcessor GetInstance() => this;

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            return compiledAssembly.InMemoryAssembly.PeData != null &&
                   compiledAssembly.InMemoryAssembly.PdbData != null &&
                   !BaseBlacklistedAssemblyNames.Contains(compiledAssembly.Name) &&
                   !ReadBlacklistedAssemblyNamesFromSettings().Contains(compiledAssembly.Name);
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            var diagnostics = new List<DiagnosticMessage>();

            try
            {
                using var assembly = ReadAssembly(compiledAssembly);

                if (IsEditorOnly(assembly, compiledAssembly.Name))
                    return new ILPostProcessResult(null, diagnostics);

                var injectedCount = HookInjection.InjectHooks(Hooks.Value, assembly);

                if (injectedCount == 0)
                    return new ILPostProcessResult(null, diagnostics);

                var pe = new MemoryStream();
                var pdb = new MemoryStream();
                assembly.Write(pe, new WriterParameters
                {
                    SymbolWriterProvider = new PortablePdbWriterProvider(),
                    SymbolStream = pdb,
                    WriteSymbols = true
                });

                return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), diagnostics);
            }
            catch (Exception e)
            {
                // Never break the user's compilation because of the recorder: keep the assembly as compiled.
                diagnostics.Add(new DiagnosticMessage
                {
                    DiagnosticType = DiagnosticType.Warning,
                    MessageData = $"[PLUME] Hooks were not injected in {compiledAssembly.Name}: {e.Message}"
                });
                return new ILPostProcessResult(null, diagnostics);
            }
        }

        /// <summary>
        /// Runs the regular hook registration (<see cref="IRegisterHooksCallback"/>) in the post-processing process.
        /// Types that cannot be loaded there (e.g. types depending on plugin DLLs) are skipped: hook declarations
        /// only depend on UnityEngine and the recorder. Only assemblies loaded by this process are scanned, so hooks
        /// must be declared in PLUME.Recorder: loading other assemblies here could not be undone when they change.
        /// </summary>
        private static IReadOnlyList<Hook> DiscoverHooks()
        {
            var registry = new HooksRegistry();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(type => type != null).ToArray();
                }

                foreach (var type in types)
                {
                    if (type.IsAbstract || !typeof(IRegisterHooksCallback).IsAssignableFrom(type))
                        continue;

                    ((IRegisterHooksCallback)Activator.CreateInstance(type)).RegisterHooks(registry);
                }
            }

            return registry.Hooks;
        }

        /// <summary>
        /// Whether the assembly only runs in the editor, read from its asmdef (found from the source paths stored in
        /// its debug symbols), the same source of truth Unity uses. Predefined editor assemblies have no asmdef.
        /// </summary>
        private static bool IsEditorOnly(AssemblyDefinition assembly, string assemblyName)
        {
            if (assemblyName.StartsWith("Assembly-CSharp-Editor", StringComparison.Ordinal))
                return true;

            // Source generators produce documents with virtual paths: only real files locate the asmdef.
            var sourcePath = assembly.MainModule.Types
                .SelectMany(type => type.Methods)
                .Where(method => method.DebugInformation.HasSequencePoints)
                .Select(method => method.DebugInformation.SequencePoints[0].Document.Url)
                .FirstOrDefault(File.Exists);

            if (sourcePath == null)
                return false;

            for (var dir = Path.GetDirectoryName(sourcePath); !string.IsNullOrEmpty(dir); dir = Path.GetDirectoryName(dir))
            {
                var asmdef = Directory.GetFiles(dir, "*.asmdef").FirstOrDefault();
                if (asmdef == null)
                    continue;

                var includePlatforms = Regex.Match(File.ReadAllText(asmdef),
                    "\"includePlatforms\"\\s*:\\s*\\[(?<platforms>[^\\]]*)\\]").Groups["platforms"].Value;
                var platforms = Regex.Matches(includePlatforms, "\"(?<platform>[^\"]+)\"")
                    .Cast<Match>().Select(match => match.Groups["platform"].Value).ToArray();
                return platforms.Length == 1 && platforms[0] == "Editor";
            }

            return false;
        }

        /// <summary>
        /// Reads the user blacklist from the hooks settings asset. The post-processor cannot load assets, so the
        /// serialized YAML list is parsed directly.
        /// </summary>
        private static HashSet<string> ReadBlacklistedAssemblyNamesFromSettings()
        {
            var names = new HashSet<string>();

            if (!File.Exists(HooksSettingsAssetPath))
                return names;

            var inList = false;
            foreach (var line in File.ReadLines(HooksSettingsAssetPath))
            {
                if (line.TrimStart().StartsWith("blacklistedAssemblyNames:", StringComparison.Ordinal))
                {
                    inList = true;
                    continue;
                }

                if (!inList)
                    continue;

                var item = line.TrimStart();
                if (!item.StartsWith("- ", StringComparison.Ordinal))
                    break;

                names.Add(item.Substring(2).Trim());
            }

            return names;
        }

        private static AssemblyDefinition ReadAssembly(ICompiledAssembly compiledAssembly)
        {
            var resolver = new DefaultAssemblyResolver();
            foreach (var directory in compiledAssembly.References.Select(Path.GetDirectoryName).Distinct())
                resolver.AddSearchDirectory(directory);

            return AssemblyDefinition.ReadAssembly(
                new MemoryStream(compiledAssembly.InMemoryAssembly.PeData.ToArray()),
                new ReaderParameters
                {
                    ReadingMode = ReadingMode.Immediate,
                    AssemblyResolver = resolver,
                    ReadSymbols = true,
                    SymbolStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData.ToArray()),
                    SymbolReaderProvider = new PortablePdbReaderProvider(),
                    ReflectionImporterProvider = new CoreLibReflectionImporterProvider()
                });
        }
    }
}
