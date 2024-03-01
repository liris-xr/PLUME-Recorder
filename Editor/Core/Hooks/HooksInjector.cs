using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using Mono.Collections.Generic;
using PLUME.Base.Hooks;
using Unity.Jobs;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Logger = PLUME.Core.Logger;
using Object = UnityEngine.Object;

namespace PLUME.Editor.Core.Hooks
{
    public class HooksInjector
    {
        private static HooksInjector _instance;
        private List<MethodDetour> _detours;

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
            // _detours = HooksManager.GetRegisteredHooks();

            var goCtor = typeof(GameObject).GetConstructor(new[] { typeof(string) });
            var goCtorDetour = typeof(TestHooks).GetMethod(nameof(TestHooks.GameObjectCtorDetour));
            
            // Get generic method GameObject::AddComponent<T>
            var goAddComponent = typeof(GameObject).GetMethod(nameof(GameObject.AddComponent), Type.EmptyTypes);
            var goAddComponentDetour = typeof(TestHooks).GetMethod(nameof(TestHooks.AddComponentDetour));
            
            // Get generic method Object::FindFirstObjectByType<T>
            var objFindFirstObjectByType = typeof(Object).GetMethod(nameof(Object.FindFirstObjectByType), Type.EmptyTypes);
            var objFindFirstObjectByTypeDetour = typeof(TestHooks).GetMethod(nameof(TestHooks.FindFirstObjectByTypeDetour));
            
            // Get generic method GameObject::GetComponentsInChildren<T>(List<T>)
            var goGetComponentsInChildren = typeof(GameObject)
                .GetMethods().First(m => m.Name == nameof(GameObject.GetComponentsInChildren) && m.ContainsGenericParameters && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType != typeof(bool));
            var goGetComponentsInChildrenDetour = typeof(TestHooks).GetMethod(nameof(TestHooks.GetComponentsInChildrenDetour));
            
            _detours = new List<MethodDetour>
            {
                new("game_object_ctor", goCtor, goCtorDetour),
                new("game_object_add_component", goAddComponent, goAddComponentDetour),
                new("object_find_first_object_by_type", objFindFirstObjectByType, objFindFirstObjectByTypeDetour),
                new("game_object_get_components_in_children", goGetComponentsInChildren, goGetComponentsInChildrenDetour)
            };

            var sb = new StringBuilder();
            sb.AppendLine("Hooks registered:");

            foreach (var hook in _detours)
            {
                var targetName = $"{hook.TargetMethod.DeclaringType?.Name}::{hook.TargetMethod}";
                sb.AppendLine($"{hook.Name} registered for target {targetName}");
            }

            Logger.Log(sb.ToString());

            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
        }

        private void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            // var assemblies = new[] { "Runtime.dll" };
            // var assemblies = new[] { "Assembly-CSharp.dll", "Runtime.dll" };
            var assemblies = new[] { "Assembly-CSharp.dll", "Runtime.dll", "Unity.XR.Interaction.Toolkit.dll" };
            
            var shouldInjectInAssembly = assemblies.Any(assemblyPath.EndsWith);

            if (!shouldInjectInAssembly)
                return;

            var results = InjectDetoursInAssembly(_detours, assemblyPath);

            if (results.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Hooks injected in {assemblyPath}:");

                var maxHookLength = results.Max(result => result.HookName.Length);

                foreach (var result in results)
                {
                    sb.AppendLine(
                        $"{result.HookName.PadRight(maxHookLength)} \t -> {result.MethodName}::{result.Instruction}");
                }

                Logger.Log(sb.ToString());
            }
            else
            {
                Logger.Log($"No detours injected in {assemblyPath}");
            }
        }

        private static List<HooksInjectorResult> InjectDetoursInAssembly(IReadOnlyCollection<MethodDetour> detours,
            string assemblyPath)
        {
            var results = new List<HooksInjectorResult>();

            using var assemblyStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.ReadWrite);
            using var assemblyDefinition =
                AssemblyDefinition.ReadAssembly(assemblyStream, GetReaderParameters(assemblyPath));

            var collectedTypes = new Collection<TypeDefinition>();

            CollectTypesRecursively(assemblyDefinition.MainModule.Types, ShouldIncludeType, collectedTypes);

            foreach (var method in collectedTypes.SelectMany(typeDefinition => typeDefinition.Methods))
            {
                results.AddRange(InjectDetoursInMethod(detours, method));
            }

            assemblyDefinition.Write(GetWriterParameters());
            assemblyDefinition.Dispose();
            assemblyStream.Dispose();

            return results;
        }

        private static IEnumerable<HooksInjectorResult> InjectDetoursInMethod(IReadOnlyCollection<MethodDetour> detours,
            MethodDefinition methodDefinition)
        {
            var results = new List<HooksInjectorResult>();

            if (!methodDefinition.HasBody)
                return results;

            var processor = methodDefinition.Body.GetILProcessor();
            var instructions = methodDefinition.Body.Instructions;

            var instructionIdx = 0;

            while (instructionIdx < instructions.Count)
            {
                foreach (var methodDetour in detours)
                {
                    var instruction = instructions[instructionIdx];
                    var success = methodDetour.TryInjectAt(instruction, processor);
                    if (!success) continue;
                    results.Add(new HooksInjectorResult(methodDetour.Name, methodDefinition.FullName, instruction));
                    break;
                }

                instructionIdx++;
            }

            return results;
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

        private static bool ShouldIncludeType(TypeReference typeReference)
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

            var jobRef = typeReference.Module.ImportReference(typeof(IJob)).Resolve();
            var isJob = typeDefinition.IsAssignableFrom(jobRef);

            return !isJob;
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
    }
}