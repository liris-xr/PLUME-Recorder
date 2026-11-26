using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PLUME.Core.Hooks
{
    public class HooksRegistry
    {
        public IReadOnlyList<Hook> Hooks => _hooks;

        /// <summary>
        /// Why hooks were skipped: a target missing or changed in this Unity or package version, a callback that
        /// failed. The other hooks are still registered.
        /// </summary>
        public IReadOnlyList<string> Errors => _errors;

        private readonly List<Hook> _hooks = new();
        private readonly List<string> _errors = new();

        internal void RegisterHooksInternal()
        {
            var registerHooksCallbackAssembly = typeof(IRegisterHooksCallback).Assembly;
            RegisterHooksFrom(AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a == registerHooksCallbackAssembly || a.GetReferencedAssemblies()
                    .Any(ra => ra.FullName == registerHooksCallbackAssembly.GetName().FullName)));
        }

        /// <summary>
        /// Runs every <see cref="IRegisterHooksCallback"/> of the assemblies. Types that cannot be loaded and
        /// callbacks that fail are skipped and reported in <see cref="Errors"/>.
        /// </summary>
        internal void RegisterHooksFrom(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
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

                    try
                    {
                        ((IRegisterHooksCallback)Activator.CreateInstance(type)).RegisterHooks(this);
                    }
                    catch (Exception e)
                    {
                        _errors.Add($"Hooks of {type.FullName} were not all registered: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Registers a hook, or skips it with an error if its target is missing (e.g. an API removed or renamed in
        /// this Unity version) or incompatible.
        /// </summary>
        public void RegisterHook(MethodInfo hookMethod, MethodBase targetMethod)
        {
            try
            {
                CheckHookMethodValidity(hookMethod, targetMethod);
            }
            catch (Exception e)
            {
                _errors.Add($"Skipped hook {hookMethod?.DeclaringType?.Name}.{hookMethod?.Name}: {e.Message}");
                return;
            }

            _hooks.Add(new Hook(hookMethod, targetMethod));
        }

        internal static void CheckHookMethodValidity(MethodInfo hookMethod, MethodBase targetMethod)
        {
            if(hookMethod == null)
                throw new ArgumentNullException(nameof(hookMethod));
            
            if(targetMethod == null)
                throw new ArgumentNullException(nameof(targetMethod));
            
            CheckGenericArgumentsValidity(hookMethod, targetMethod);
            CheckParametersValidity(hookMethod, targetMethod);
            CheckReturnTypeValidity(hookMethod, targetMethod);
        }

        private static void CheckReturnTypeValidity(MethodInfo hookMethod, MethodBase targetMethod)
        {
            var targetReturnType = targetMethod switch
            {
                MethodInfo targetMethodInfo => targetMethodInfo.ReturnType,
                ConstructorInfo targetConstructorInfo => targetConstructorInfo.DeclaringType!,
                _ => throw new NotImplementedException()
            };

            // If targetMethod.ReturnType is a generic type, check that it is the same generic type as hookMethod.ReturnType
            if (hookMethod.ReturnType.IsGenericParameter != targetReturnType.IsGenericParameter)
                throw new InvalidHookMethodException(hookMethod, targetMethod,
                    "Expected return type {targetMethod.ReturnType}, got {hookMethod.ReturnType}.");

            if (targetReturnType.IsGenericParameter)
            {
                var targetGenericParamConstraints = targetReturnType.GetGenericParameterConstraints();
                var hookGenericParamConstraints = hookMethod.ReturnType.GetGenericParameterConstraints();

                if (!hookGenericParamConstraints.SequenceEqual(targetGenericParamConstraints))
                    throw new InvalidHookMethodException(hookMethod, targetMethod,
                        $"Expected return type {targetReturnType}, got {hookMethod.ReturnType}.");
            }
            else if (!hookMethod.ReturnType.IsAssignableFrom(targetReturnType))
            {
                throw new InvalidHookMethodException(hookMethod, targetMethod,
                    $"Expected return type {targetReturnType}, got {hookMethod.ReturnType}.");
            }
        }

        private static void CheckParametersValidity(MethodInfo hookMethod, MethodBase targetMethod)
        {
            var hookParams = hookMethod.GetParameters();
            var targetParamsTypes = targetMethod.GetParameters().Select(p => p.ParameterType).ToList();

            var hasThis = targetMethod.CallingConvention.HasFlag(CallingConventions.HasThis);
            
            if (hasThis && targetMethod is not ConstructorInfo)
            {
                targetParamsTypes.Insert(0, targetMethod.DeclaringType);
            }
            
            if (hookParams.Length != targetParamsTypes.Count)
                throw new InvalidHookMethodException(hookMethod, targetMethod,
                    $"Expected {targetParamsTypes.Count} parameters, got {hookParams.Length}.");

            for (var i = 0; i < hookParams.Length; i++)
            {
                if (hookParams[i].ParameterType.IsGenericParameter != targetParamsTypes[i].IsGenericParameter)
                {
                    throw new InvalidHookMethodException(hookMethod, targetMethod,
                        $"Expected parameter {targetParamsTypes[i]}, got {hookParams[i].ParameterType}");
                }

                if (targetParamsTypes[i].IsGenericParameter)
                {
                    var targetGenericParamConstraints = targetParamsTypes[i].GetGenericParameterConstraints();
                    var hookGenericParamConstraints = hookParams[i].ParameterType.GetGenericParameterConstraints();

                    if (!hookGenericParamConstraints.SequenceEqual(targetGenericParamConstraints))
                        throw new InvalidHookMethodException(hookMethod, targetMethod,
                            $"Expected parameter {targetParamsTypes[i]}, got {hookParams[i].ParameterType}");
                }
                else if (!hookParams[i].ParameterType.IsAssignableFrom(targetParamsTypes[i]))
                {
                    throw new InvalidHookMethodException(hookMethod, targetMethod,
                        $"Expected parameter {targetParamsTypes[i]}, got {hookParams[i].ParameterType}");
                }
            }
        }

        private static void CheckGenericArgumentsValidity(MethodInfo hookMethod, MethodBase targetMethod)
        {
            var hookGenericArgs = hookMethod.GetGenericArguments();
            var targetGenericArgs =
                targetMethod is ConstructorInfo ? Type.EmptyTypes : targetMethod.GetGenericArguments();

            if (hookGenericArgs.Length != targetGenericArgs.Length)
                throw new InvalidHookMethodException(hookMethod, targetMethod,
                    $"Expected {targetGenericArgs.Length} generic parameters, got {hookGenericArgs.Length}.");

            for (var i = 0; i < hookGenericArgs.Length; i++)
            {
                var hookGenericParamConstraints = hookGenericArgs[i].GetGenericParameterConstraints();
                var targetGenericParamConstraints = targetGenericArgs[i].GetGenericParameterConstraints();

                if (!hookGenericParamConstraints.SequenceEqual(targetGenericParamConstraints))
                    throw new InvalidHookMethodException(hookMethod, targetMethod,
                        $"Expected generic parameter {targetGenericArgs[i]}, got {hookGenericArgs[i]}");
            }
        }
    }
}