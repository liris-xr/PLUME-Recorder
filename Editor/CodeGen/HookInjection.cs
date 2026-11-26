using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PLUME.Core.Hooks;

namespace PLUME.Editor.CodeGen
{
    /// <summary>
    /// Replaces calls to hooked methods by calls to their hooks in an assembly.
    /// </summary>
    internal static class HookInjection
    {
        /// <summary>
        /// Replaces every call, callvirt and newobj targeting a hooked method by a call to its hook.
        /// </summary>
        /// <returns>The number of replaced instructions.</returns>
        public static int InjectHooks(IReadOnlyList<Hook> hooks, AssemblyDefinition assembly)
        {
            var module = assembly.MainModule;
            var resolvedHooksByTargetName = new Dictionary<string, List<ResolvedHook>>();

            foreach (var hook in hooks)
            {
                ResolvedHook resolvedHook;
                try
                {
                    var targetMethodDefinition = module.ImportReference(hook.TargetMethod).Resolve();

                    // Not resolvable from this assembly's references: the assembly cannot call it anyway.
                    if (targetMethodDefinition == null)
                        continue;

                    resolvedHook = new ResolvedHook(module.ImportReference(hook.HookMethod), targetMethodDefinition);
                }
                catch (AssemblyResolutionException)
                {
                    continue;
                }

                if (!resolvedHooksByTargetName.TryGetValue(resolvedHook.TargetMethodDefinition.Name, out var resolvedHooks))
                {
                    resolvedHooks = new List<ResolvedHook>();
                    resolvedHooksByTargetName[resolvedHook.TargetMethodDefinition.Name] = resolvedHooks;
                }

                resolvedHooks.Add(resolvedHook);
            }

            return GetAllMethods(assembly).Sum(method => InjectHooksInMethod(resolvedHooksByTargetName, method));
        }

        private static int InjectHooksInMethod(Dictionary<string, List<ResolvedHook>> resolvedHooksByTargetName,
            MethodDefinition methodDefinition)
        {
            if (!methodDefinition.HasBody)
                return 0;

            var injectedCount = 0;
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
                    var targetMethodDef = resolvedHook.TargetMethodDefinition;

                    // Quick heuristic to check if the instruction is a call to the target method
                    if (instrMethodRef.Parameters.Count != targetMethodDef.Parameters.Count ||
                        instrMethodRef.Name != targetMethodDef.Name ||
                        instrMethodRef.DeclaringType.Name != targetMethodDef.DeclaringType.Name)
                        continue;

                    // Strict check to make sure the instruction is a call to the target method
                    if (instrMethodRef.Resolve() != targetMethodDef)
                        continue;

                    // A constrained. callvirt (a call on a generic receiver) cannot become a call: left unhooked.
                    if (instruction.Previous?.OpCode == OpCodes.Constrained)
                        break;

                    if (instrMethodRef is GenericInstanceMethod genericInstanceMethod)
                    {
                        var genericHookMethodRef = new GenericInstanceMethod(resolvedHook.HookMethodReference);

                        foreach (var genericArgument in genericInstanceMethod.GenericArguments)
                            genericHookMethodRef.GenericArguments.Add(genericArgument);

                        foreach (var genericParameter in genericInstanceMethod.GenericParameters)
                            genericHookMethodRef.GenericParameters.Add(genericParameter);

                        instruction.Operand = genericHookMethodRef;
                    }
                    else
                    {
                        instruction.Operand = resolvedHook.HookMethodReference;
                    }

                    // Rewritten in place: branches and sequence points pointing at the instruction stay valid.
                    instruction.OpCode = OpCodes.Call;

                    injectedCount++;
                    break;
                }
            }

            return injectedCount;
        }

        private static IEnumerable<MethodDefinition> GetAllMethods(AssemblyDefinition assembly)
        {
            return assembly.Modules.SelectMany(module => GetTypesRecursively(module.Types))
                .SelectMany(type => type.Methods).Distinct();
        }

        private static IEnumerable<TypeDefinition> GetTypesRecursively(IEnumerable<TypeDefinition> types)
        {
            foreach (var type in types)
            {
                yield return type;

                foreach (var nestedType in GetTypesRecursively(type.NestedTypes))
                    yield return nestedType;
            }
        }

        private readonly struct ResolvedHook
        {
            public readonly MethodReference HookMethodReference;
            public readonly MethodDefinition TargetMethodDefinition;

            public ResolvedHook(MethodReference hookMethodReference, MethodDefinition targetMethodDefinition)
            {
                HookMethodReference = hookMethodReference;
                TargetMethodDefinition = targetMethodDefinition;
            }
        }
    }

    /// <summary>
    /// The post-processor runs on .NET, where reflected core types live in System.Private.CoreLib: references to
    /// it must be redirected to the core library the compiled assembly actually references.
    /// </summary>
    internal sealed class CoreLibReflectionImporterProvider : IReflectionImporterProvider
    {
        public IReflectionImporter GetReflectionImporter(ModuleDefinition module) => new CoreLibReflectionImporter(module);

        private sealed class CoreLibReflectionImporter : DefaultReflectionImporter
        {
            private readonly AssemblyNameReference _coreLibrary;

            public CoreLibReflectionImporter(ModuleDefinition module) : base(module)
            {
                _coreLibrary = module.AssemblyReferences.FirstOrDefault(reference =>
                    reference.Name is "mscorlib" or "netstandard" or "System.Private.CoreLib");
            }

            public override AssemblyNameReference ImportReference(AssemblyName name)
            {
                return _coreLibrary != null && name.Name == "System.Private.CoreLib"
                    ? _coreLibrary
                    : base.ImportReference(name);
            }
        }
    }
}
