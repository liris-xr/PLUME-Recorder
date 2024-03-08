using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using PLUME.Editor.Core.Events;
using UnityEditor;
using UnityEditor.Compilation;
using Logger = PLUME.Core.Logger;

namespace PLUME.Editor.Core.Weaving
{
    public class MethodDetourInjector
    {
        private static MethodDetourInjector _instance;
        private List<MethodDetour> _detours;

        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            if (_instance != null)
                CompilationPipeline.assemblyCompilationFinished -= _instance.OnAssemblyCompilationFinished;
            _instance = new MethodDetourInjector();
            _instance.Initialize();
        }

        private void Initialize()
        {
            _detours = MethodDetourManager.GetRegisteredMethodDetours();

#if PLUME_LOG_REGISTERED_DETOURS
            var sb = new StringBuilder();
            sb.AppendLine("Detours registered:");

            foreach (var hook in _detours)
            {
                var targetName = $"{hook.TargetMethod.DeclaringType?.Name}::{hook.TargetMethod}";
                sb.AppendLine($"{hook.DetourMethod} registered for target {targetName}");
            }

            Logger.Log(sb.ToString());
#endif

            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
        }

        private void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            var assemblies = new[] { "Assembly-CSharp.dll", "Unity.XR.Interaction.Toolkit.dll", "Unity.VisualScripting.Core.dll" };

            var shouldInjectInAssembly = assemblies.Any(Path.GetFileName(assemblyPath).Equals);

            if (!shouldInjectInAssembly)
                return;

            var results = InjectDetoursInAssembly(_detours, assemblyPath);

            if (results.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Hooks injected in {assemblyPath}:");

                var maxHookLength = results.Max(result => result.Detour.DetourMethod.Name.Length);

                foreach (var result in results)
                {
                    sb.AppendLine(
                        $"{result.Detour.DetourMethod.Name.PadRight(maxHookLength)} \t -> {result.InstructionMethod.FullName}::{result.Instruction}");
                }

                Logger.Log(sb.ToString());
            }
            else
            {
                Logger.Log($"No detours injected in {assemblyPath}");
            }
        }

        private static List<MethodDetourInjectionResult> InjectDetoursInAssembly(
            IReadOnlyCollection<MethodDetour> detours,
            string assemblyPath)
        {
            var results = new List<MethodDetourInjectionResult>();

            using var assemblyStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.ReadWrite);
            using var assemblyDefinition =
                AssemblyDefinition.ReadAssembly(assemblyStream, GetReaderParameters(assemblyPath));

            var methods = assemblyDefinition.GetAllMethods();

            foreach (var method in methods)
            {
                results.AddRange(InjectDetoursInMethod(detours, method));
            }

            assemblyDefinition.Write(GetWriterParameters());
            return results;
        }

        private static IEnumerable<MethodDetourInjectionResult> InjectDetoursInMethod(
            IReadOnlyCollection<MethodDetour> detours,
            MethodDefinition methodDefinition)
        {
            var results = new List<MethodDetourInjectionResult>();

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
                    var success = TryInjectMethodDetourAtInstruction(methodDetour, instruction, processor);
                    if (!success) continue;
                    results.Add(new MethodDetourInjectionResult(methodDetour, methodDefinition, instruction));
                    break;
                }

                instructionIdx++;
            }

            return results;
        }

        private static bool TryInjectMethodDetourAtInstruction(MethodDetour methodDetour, Instruction instruction,
            ILProcessor processor)
        {
            var module = processor.Body.Method.Module;

            if (!InstructionMatchesTarget(methodDetour.TargetMethod, module, instruction))
                return false;

            var hookMethodRef = module.ImportReference(methodDetour.DetourMethod);
            var targetMethodRef = instruction.Operand as MethodReference;

            if (instruction.OpCode != OpCodes.Call &&
                instruction.OpCode != OpCodes.Callvirt &&
                instruction.OpCode != OpCodes.Newobj)
                return false;

            if (targetMethodRef is GenericInstanceMethod genericInstanceMethod)
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
                return true;
            }

            processor.Replace(instruction, processor.Create(OpCodes.Call, hookMethodRef));
            return true;
        }

        private static bool InstructionMatchesTarget(MethodBase targetMethod, ModuleDefinition module,
            Instruction instruction)
        {
            if (instruction.OpCode != OpCodes.Call &&
                instruction.OpCode != OpCodes.Callvirt &&
                instruction.OpCode != OpCodes.Newobj)
                return false;

            if (instruction.Operand is not MethodReference instrMethodRef)
                return false;

            if (instrMethodRef.Name != targetMethod.Name)
                return false;

            if (instrMethodRef.DeclaringType?.Name != targetMethod.DeclaringType?.Name)
                return false;

            if (instrMethodRef.DeclaringType?.Namespace != targetMethod.DeclaringType?.Namespace)
                return false;

            if (instrMethodRef is not GenericInstanceMethod && targetMethod.IsGenericMethod)
                return false;

            if (instrMethodRef.Parameters.Count != targetMethod.GetParameters().Length)
                return false;

            for (var i = 0; i < instrMethodRef.Parameters.Count; i++)
            {
                var parameter = instrMethodRef.Parameters[i];
                var expectedParameter = targetMethod.GetParameters()[i];

                if (expectedParameter.ParameterType.IsGenericType)
                {
                    var parameterTypeDef = parameter.ParameterType.Resolve();
                    var expectedParamTypeDef =
                        module.ImportReference(expectedParameter.ParameterType.GetGenericTypeDefinition()).Resolve();

                    if (!parameterTypeDef.IsAssignableFrom(expectedParamTypeDef))
                        return false;
                }
                else
                {
                    // TODO: if generic, check that constraints are compatible
                    if (parameter.ParameterType is not GenericParameter)
                    {
                        var parameterTypeDef = parameter.ParameterType.Resolve();
                        var expectedParamTypeDef = module.ImportReference(expectedParameter.ParameterType).Resolve();
                    
                        if (!expectedParamTypeDef.IsAssignableFrom(parameterTypeDef))
                            return false;
                    }
                }
            }

            if (instrMethodRef is not GenericInstanceMethod genericMethodRef)
                return true;

            // TODO: check that all argument types are compatible

            return genericMethodRef.GenericArguments.Count == targetMethod.GetGenericArguments().Length;
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