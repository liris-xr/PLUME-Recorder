using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using PLUME.Core.Hooks;
using PLUME.Core.Settings;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using FileMode = System.IO.FileMode;
using Logger = PLUME.Core.Logger;

namespace PLUME.Editor.Core.Hooks
{
    public class HooksInjector
    {
        private static readonly string[] BaseBlacklistedAssemblyNames =
        {
            "PLUME.Recorder",
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
        
        private static HooksInjector _instance;

        private HooksRegistry _hooksRegistry;
        private HooksSettings _hooksSettings;

        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            if (_instance != null)
                CompilationPipeline.assemblyCompilationFinished -= _instance.OnAssemblyCompilationFinished;
            _instance = new HooksInjector();
            _instance.Initialize();
        }

        private void Initialize()
        {
            var settingsProvider = new FileSettingsProvider();
            _hooksSettings = settingsProvider.GetOrCreate<HooksSettings>();
            _hooksRegistry = new HooksRegistry();
            _hooksRegistry.RegisterHooksInternal();
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
        }

        private void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            var blacklist = _hooksSettings.BlacklistedAssemblyNames;
            
            var asm = CompilationPipeline.GetAssemblies().FirstOrDefault(asm => asm.outputPath == assemblyPath);

            if (asm == null)
            {
                Debug.LogWarning($"Assembly {assemblyPath} not found in compilation pipeline assemblies. Skipping.");
                return;
            }

            var isEditorOnly = (asm.flags & AssemblyFlags.EditorAssembly) != 0;

            if (isEditorOnly)
                return;

            if (BaseBlacklistedAssemblyNames.Contains(asm.name) || blacklist.Contains(asm.name))
            {
                Logger.Log($"Skipping blacklisted assembly {asm.name}");
                return;
            }

            try
            {
                var results = InjectHooksInAssembly(_hooksRegistry.Hooks, assemblyPath);
                
                if (results.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"Injected {results.Count} hooks in {assemblyPath}:");

                    var maxHookLength = results.Max(result => result.Hook.HookMethod.Name.Length);

                    foreach (var result in results)
                    {
                        sb.AppendLine(
                            $"{result.Hook.HookMethod.Name.PadRight(maxHookLength)} \t -> {result.InstructionMethod.FullName}::{result.Instruction}");
                    }

                    Logger.Log(sb.ToString());
                }
            } catch (Exception e)
            {
                Logger.LogWarning($"Failed to inject hooks in {assemblyPath}: {e.Message}");
            }
        }

        private static List<HookInjectionResult> InjectHooksInAssembly(IReadOnlyList<Hook> hooks, string assemblyPath)
        {
            var results = new List<HookInjectionResult>();

            using var assemblyStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.ReadWrite);
            using var assemblyDefinition =
                AssemblyDefinition.ReadAssembly(assemblyStream, GetReaderParameters(assemblyPath));

            var methods = assemblyDefinition.GetAllMethods();

            var resolvedHooksByTargetName = new Dictionary<string, List<ResolvedHook>>();

            foreach (var hook in hooks)
            {
                var hookMethodRef = assemblyDefinition.MainModule.ImportReference(hook.HookMethod);
                var targetMethodRef = assemblyDefinition.MainModule.ImportReference(hook.TargetMethod);
                var targetMethodDef = targetMethodRef.Resolve();
                var resolvedHook = new ResolvedHook(hook, hookMethodRef, targetMethodRef, targetMethodDef);

                if (!resolvedHooksByTargetName.TryGetValue(targetMethodDef.Name, out var resolvedHooks))
                {
                    resolvedHooks = new List<ResolvedHook>();
                    resolvedHooksByTargetName[targetMethodDef.Name] = resolvedHooks;
                }

                resolvedHooks.Add(resolvedHook);
            }

            foreach (var method in methods)
            {
                results.AddRange(InjectHooksInMethod(resolvedHooksByTargetName, method));
            }

            assemblyDefinition.Write(GetWriterParameters());
            return results;
        }

        private static IEnumerable<HookInjectionResult> InjectHooksInMethod(
            Dictionary<string, List<ResolvedHook>> resolvedHooksByTargetName, MethodDefinition methodDefinition)
        {
            var results = new List<HookInjectionResult>();

            if (!methodDefinition.HasBody)
                return results;

            var processor = methodDefinition.Body.GetILProcessor();
            var instructions = methodDefinition.Body.Instructions;

            for (var i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];

                if (instruction.OpCode != OpCodes.Call &&
                    instruction.OpCode != OpCodes.Callvirt &&
                    instruction.OpCode != OpCodes.Newobj)
                    continue;

                if (instruction.Operand is not MethodReference instrMethodRef)
                    continue;

                if (!resolvedHooksByTargetName.TryGetValue(instrMethodRef.Name, out var resolvedHooks))
                    continue;

                foreach (var resolvedHook in resolvedHooks)
                {
                    var hookMethodRef = resolvedHook.HookMethodReference;
                    var targetMethodRef = resolvedHook.TargetMethodReference;
                    var targetMethodDef = resolvedHook.TargetMethodDefinition;

                    // Quick heuristic to check if the instruction is a call to the target method
                    if (instrMethodRef.Parameters.Count != targetMethodDef.Parameters.Count ||
                        instrMethodRef.Name != targetMethodDef.Name ||
                        instrMethodRef.DeclaringType.Name != targetMethodDef.DeclaringType.Name)
                        continue;

                    // Strict check to make sure the instruction is a call to the target method
                    if (instrMethodRef.Resolve() != targetMethodDef)
                        continue;

                    if (instrMethodRef is GenericInstanceMethod genericInstanceMethod)
                    {
                        var genericHookMethodRef = new GenericInstanceMethod(hookMethodRef);

                        foreach (var genericArgument in genericInstanceMethod.GenericArguments)
                        {
                            genericHookMethodRef.GenericArguments.Add(genericArgument);
                        }

                        foreach (var genericParameter in genericInstanceMethod.GenericParameters)
                        {
                            genericHookMethodRef.GenericParameters.Add(genericParameter);
                        }

                        processor.Replace(instruction, processor.Create(OpCodes.Call, genericHookMethodRef));
                    }
                    else
                    {
                        processor.Replace(instruction,
                            processor.Create(OpCodes.Call, resolvedHook.HookMethodReference));
                    }

                    results.Add(new HookInjectionResult(resolvedHook.Hook, methodDefinition, instruction));
                    break;
                }
            }

            return results;
        }

        private static WriterParameters GetWriterParameters()
        {
            var writerParameters = new WriterParameters
            {
                WriteSymbols = true,
                SymbolWriterProvider = new PdbWriterProvider()
            };
            return writerParameters;
        }

        private static ReaderParameters GetReaderParameters(string assemblyPath)
        {
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            var assemblyResolver = new AssemblyResolver(assemblyName);

            var readerParameters = new ReaderParameters
            {
                ReadingMode = ReadingMode.Immediate,
                ReadWrite = true,
                AssemblyResolver = assemblyResolver,
                ReadSymbols = true,
                SymbolReaderProvider = new PdbReaderProvider()
            };
            return readerParameters;
        }

        private readonly struct ResolvedHook
        {
            public readonly Hook Hook;
            public readonly MethodReference HookMethodReference;
            public readonly MethodReference TargetMethodReference;
            public readonly MethodDefinition TargetMethodDefinition;

            public ResolvedHook(Hook hook, MethodReference hookMethodReference, MethodReference targetMethodReference,
                MethodDefinition targetMethodDefinition)
            {
                Hook = hook;
                HookMethodReference = hookMethodReference;
                TargetMethodReference = targetMethodReference;
                TargetMethodDefinition = targetMethodDefinition;
            }
        }
    }
}