using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PLUME.Core.Hooks
{
    public class HooksRegistry
    {
        public IReadOnlyList<Hook> Hooks => _hooks;

        private readonly List<Hook> _hooks = new();

        internal void RegisterHooksInternal()
        {
            var registerHooksCallbackAssembly = typeof(IRegisterHooksCallback).Assembly;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a == registerHooksCallbackAssembly || a.GetReferencedAssemblies()
                    .Any(ra => ra.FullName == registerHooksCallbackAssembly.GetName().FullName));
            
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetInterface(nameof(IRegisterHooksCallback)) != null)
                    {
                        var instance = Activator.CreateInstance(type) as IRegisterHooksCallback;
                        instance?.RegisterHooks(this);
                    }
                }
            }
        }

        public void RegisterHook(MethodInfo hookMethod, MethodBase targetMethod)
        {
            CheckHookMethodValidity(hookMethod, targetMethod);
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