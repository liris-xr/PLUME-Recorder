using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using Mono.Collections.Generic;
using Unity.Jobs;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Logger = PLUME.Core.Logger;

namespace PLUME.Editor.Core.Hooks
{
    public class HooksInjector
    {
        private static HooksInjector _instance;
        private readonly HooksRegistry _hooksRegistry;

        private HooksInjector()
        {
            _hooksRegistry = new HooksRegistry();
            _hooksRegistry.RegisterHooks();
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
        }

        ~HooksInjector()
        {
            CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            _instance = new HooksInjector();
        }

        private void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            if (assemblyPath.EndsWith("Assembly-CSharp.dll"))
            {
                var results = InjectHooksInAssembly(assemblyPath);

                var sb = new StringBuilder();
                sb.AppendLine("Hooks injected in Assembly-CSharp.dll:");
                
                var maxHookLength = results.Max(result => result.HookName.Length); 
                
                foreach (var result in results)
                {
                    sb.AppendLine($"{result.HookName.PadRight(maxHookLength)} \t -> {result.MethodName}::{result.Instruction}");
                }
                
                Logger.Log(sb.ToString());
            }

            if (assemblyPath.EndsWith("Unity.VisualScripting.Core.dll"))
            {
                // TODO: fix sharing violation
                // InjectHooksInAssemblyMethods(assemblyPath,
                //     Type.GetType("Unity.VisualScripting.ComponentHolderProtocol, Unity.VisualScripting.Core")?.GetMethod("AddComponent"),
                //     Type.GetType("Unity.VisualScripting.ComponentHolderProtocol, Unity.VisualScripting.Core")?.GetMethod("GetOrAddComponent")
                // );
            }
        }

        private List<HooksInjectorResult> InjectHooksInAssemblyMethods(string assemblyPath, params MethodBase[] methodInfo)
        {
            var results = new List<HooksInjectorResult>();
            
            using var assemblyStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.ReadWrite);
            using var assemblyDefinition =
                AssemblyDefinition.ReadAssembly(assemblyStream, GetReaderParameters(assemblyPath));

            foreach (var method in methodInfo)
            {
                var methodDefinition = assemblyDefinition.MainModule.ImportReference(method).Resolve();
                results.AddRange(InjectHooksInMethod(methodDefinition));
            }

            assemblyDefinition.Write(GetWriterParameters());
            return results;
        }

        private List<HooksInjectorResult> InjectHooksInAssembly(string assemblyPath)
        {
            var results = new List<HooksInjectorResult>();
            
            using var assemblyStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.ReadWrite);
            using var assemblyDefinition =
                AssemblyDefinition.ReadAssembly(assemblyStream, GetReaderParameters(assemblyPath));

            var collectedTypes = new Collection<TypeDefinition>();

            CollectTypesRecursively(assemblyDefinition.MainModule.Types, ShouldIncludeType, collectedTypes);

            foreach (var method in collectedTypes.SelectMany(typeDefinition => typeDefinition.Methods))
            {
                results.AddRange(InjectHooksInMethod(method));
            }

            assemblyDefinition.Write(GetWriterParameters());
            
            return results;
        }

        private List<HooksInjectorResult> InjectHooksInMethod(MethodDefinition methodDefinition)
        {
            var results = new List<HooksInjectorResult>();

            if (!methodDefinition.HasBody)
                return results;

            var worker = methodDefinition.Body.GetILProcessor();
            var instructions = methodDefinition.Body.Instructions;

            var instructionIdx = 0;

            while (instructionIdx < instructions.Count)
            {
                foreach (var hook in _hooksRegistry.RegisteredHooks)
                {
                    var instruction = instructions[instructionIdx];

                    var prevInstructionsCount = instructions.Count;
                    var success = hook.TryInjectAtInstruction(worker, instruction);
                    var newInstructionsCount = instructions.Count;
                    var instructionsAdded = newInstructionsCount - prevInstructionsCount;

                    // Prevent infinite loop if the hook added instructions, especially if it added instructions before the current instruction
                    instructionIdx += instructionsAdded;

                    if (!success) continue;
                    results.Add(new HooksInjectorResult(hook.Name, methodDefinition.FullName, instruction));
                    break;
                }

                instructionIdx++;
            }
            
            return results;
        }

        private void CollectTypesRecursively(
            IEnumerable<TypeDefinition> types,
            Predicate<TypeReference> predicate,
            ICollection<TypeDefinition> collectedTypes)
        {
            foreach (var typeDefinition in types)
            {
                if (!predicate(typeDefinition)) continue;

                collectedTypes.Add(typeDefinition);

                if (typeDefinition.HasNestedTypes)
                {
                    CollectTypesRecursively(typeDefinition.NestedTypes, predicate, collectedTypes);
                }
            }
        }

        private bool ShouldIncludeType(TypeReference typeReference)
        {
            if (typeReference.Name == "<Module>" ||
                typeReference.Name == "<PrivateImplementationDetails>" ||
                typeReference.Name.StartsWith("UnitySourceGeneratedAssemblyMonoScriptTypes") ||
                typeReference.Name.StartsWith("__JobReflectionRegistrationOutput__"))
                return false;

            var typeDefinition = typeReference.Resolve();

            if (!typeDefinition.HasMethods && !typeDefinition.HasProperties)
                return false;

            var burstCompiled = typeDefinition.CustomAttributes.Any(attr =>
                attr.AttributeType.Name.Contains("BurstCompileAttribute"));

            if (burstCompiled)
                return false;

            var isJob = typeReference.IsAssignableFrom(typeof(IJob));

            return !isJob;
        }

        private WriterParameters GetWriterParameters()
        {
            var writerParameters = new WriterParameters
            {
                WriteSymbols = true,
                SymbolWriterProvider = new PdbWriterProvider()
            };
            return writerParameters;
        }

        private ReaderParameters GetReaderParameters(string assemblyPath)
        {
            var readerParameters = new ReaderParameters
            {
                ReadingMode = ReadingMode.Immediate,
                ReadWrite = true,
                AssemblyResolver = new AssemblyResolver(assemblyPath),
                ReadSymbols = true,
                SymbolReaderProvider = new PdbReaderProvider()
            };
            return readerParameters;
        }
    }
}