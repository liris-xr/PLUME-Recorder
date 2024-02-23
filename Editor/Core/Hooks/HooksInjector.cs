using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    [InitializeOnLoad]
    public static class HooksInjector
    {
        private static readonly HooksRegistry HooksRegistry = new();

        static HooksInjector()
        {
            HooksRegistry.RegisterHooks();
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
        }

        private static void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            if (assemblyPath.EndsWith("Assembly-CSharp.dll"))
            {
                InjectHooksInAssembly(assemblyPath);
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

        private static void InjectHooksInAssemblyMethods(string assemblyPath, params MethodBase[] methodInfo)
        {
            using var assemblyStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.ReadWrite);
            using var assemblyDefinition =
                AssemblyDefinition.ReadAssembly(assemblyStream, GetReaderParameters(assemblyPath));

            foreach (var method in methodInfo)
            {
                var methodDefinition = assemblyDefinition.MainModule.ImportReference(method).Resolve();
                InjectHooksInMethod(methodDefinition);
            }

            assemblyDefinition.Write(GetWriterParameters());
        }

        private static void InjectHooksInAssembly(string assemblyPath)
        {
            using var assemblyStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.ReadWrite);
            using var assemblyDefinition =
                AssemblyDefinition.ReadAssembly(assemblyStream, GetReaderParameters(assemblyPath));

            var collectedTypes = new Collection<TypeDefinition>();

            CollectTypesRecursively(assemblyDefinition.MainModule.Types, IncludeType, collectedTypes);

            foreach (var method in collectedTypes.SelectMany(typeDefinition => typeDefinition.Methods))
            {
                InjectHooksInMethod(method);
            }

            assemblyDefinition.Write(GetWriterParameters());
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

        private static void InjectHooksInMethod(MethodDefinition methodDefinition)
        {
            if (!methodDefinition.HasBody)
                return;

            var worker = methodDefinition.Body.GetILProcessor();
            var instructions = methodDefinition.Body.Instructions;

            var instructionIdx = 0;

            while (instructionIdx < instructions.Count)
            {
                foreach (var hook in HooksRegistry.RegisteredHooks)
                {
                    var instruction = instructions[instructionIdx];

                    var prevInstructionsCount = instructions.Count;
                    var success = hook.TryInjectAtInstruction(worker, instruction);
                    var newInstructionsCount = instructions.Count;
                    var instructionsAdded = newInstructionsCount - prevInstructionsCount;

                    // Prevent infinite loop if the hook added instructions, especially if it added instructions before the current instruction
                    instructionIdx += instructionsAdded;

                    if (!success) continue;
                    Logger.Log(
                        $"Successfully injected {hook.Name} hook in {methodDefinition.FullName} after instruction {instruction}");
                    break;
                }

                instructionIdx++;
            }
        }

        private static void CollectTypesRecursively(
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

        private static bool IncludeType(TypeReference typeReference)
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
    }
}