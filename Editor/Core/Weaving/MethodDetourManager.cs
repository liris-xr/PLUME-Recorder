using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PLUME.Core;

namespace PLUME.Editor.Core.Events
{
    internal static class MethodDetourManager
    {
        internal static List<MethodDetour> GetRegisteredMethodDetours()
        {
            var detours = new List<MethodDetour>();

            // Find all methods with the RegisterHookAttribute and register them
            // Get all assemblies referencing assembly PLUME.Recorder
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var runtimeAsm = assemblies.First(asm => asm.GetName().Name == "PLUME.Recorder");

            var registerMethodDetourAttributes = assemblies
                .Where(asm => asm == runtimeAsm || asm.GetReferencedAssemblies()
                    .Any(asmName => asmName.Name == runtimeAsm.GetName().Name))
                .SelectMany(asm => asm.GetTypes())
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                .Select(m => new
                {
                    method = m, attribute = m.GetCustomAttributes<RegisterMethodDetourAttribute>().FirstOrDefault()
                })
                .Where(v => v.attribute != null);

            foreach (var result in registerMethodDetourAttributes)
            {
                var detourMethod = result.method;
                var targetMethod = result.attribute.TargetMethod;

                if (!result.method.IsPublic)
                {
                    Logger.LogWarning(
                        $"{result.method} is not public. Only public methods can be registered as hooks. Skipping hook.");
                }
                else if (result.attribute.TargetMethod == null)
                {
                    Logger.LogWarning($"{result.method} target method not found. Skipping hook.");
                }
                else
                {
                    if (result.attribute.TargetMethod == null)
                        throw new ArgumentNullException(nameof(result.attribute.TargetMethod));
                    if (result.method == null)
                        throw new ArgumentNullException(nameof(result.method));

                    CheckHookSignatureValidity(result.method, result.attribute.TargetMethod);

                    var detour = new MethodDetour(targetMethod, detourMethod);
                    detours.Add(detour);
                }
            }

            return detours;
        }

        private static void CheckHookSignatureValidity(MethodInfo detourMethod, MethodBase targetMethod)
        {
            // Check if the detour return type is the same as the target method
            var expectedReturnType = GetExpectedReturnType(targetMethod);
            
            if (!IsReturnTypeValid(detourMethod.ReturnType, expectedReturnType))
            {
                throw new ArgumentException(
                    $"Invalid return type for detour method {detourMethod}. Expected {expectedReturnType}.");
            }

            var detourMethodParameters = detourMethod.GetParameters();
            var expectedParametersType = GetExpectedParametersType(targetMethod);
            var expectedSignature = GetExpectedSignature(expectedParametersType, expectedReturnType);

            for (var i = 0; i < expectedParametersType.Count; i++)
            {
                if (i >= detourMethodParameters.Length)
                    throw new ArgumentException(
                        $"Invalid parameter count for detour method {detourMethod}. Expected signature is {expectedSignature}.");

                var parameterType = detourMethodParameters[i].ParameterType;
                var expectedParameterType = expectedParametersType[i];

                if (parameterType.Assembly != expectedParameterType.Assembly ||
                    parameterType.Namespace != expectedParameterType.Namespace &&
                    parameterType.Name != expectedParameterType.Name)
                    throw new ArgumentException(
                        $"Invalid parameter type at index {i} for detour method {detourMethod}. Expected signature is {expectedSignature}.");
            }
        }

        private static bool IsReturnTypeValid(Type returnType, Type expectedReturnType)
        {
            if (expectedReturnType.IsGenericParameter && returnType.IsGenericParameter)
            {
                // TODO: Be more strict with generic parameters
                return true;
            }

            if (expectedReturnType.IsGenericParameter != returnType.IsGenericParameter)
            {
                return false;
            }

            return returnType.Assembly == expectedReturnType.Assembly &&
                   (returnType.Namespace == expectedReturnType.Namespace ||
                    returnType.Name == expectedReturnType.Name);
        }

        private static string GetExpectedSignature(IEnumerable<Type> expectedParametersType, Type expectedReturnType)
        {
            var expectedParamsArr = expectedParametersType
                .Select((p, i) => $"{p} param{i}")
                .ToList();

            if (expectedReturnType != typeof(void))
                expectedParamsArr.Add($"{expectedReturnType} result");

            return $"{typeof(void)} MethodName({string.Join(", ", expectedParamsArr)})";
        }

        private static Type GetExpectedReturnType(MethodBase targetMethod)
        {
            var targetReturnType = targetMethod switch
            {
                ConstructorInfo ctor => ctor.DeclaringType,
                MethodInfo methodInfo => methodInfo.ReturnType,
                _ => typeof(void)
            };
            return targetReturnType;
        }

        private static List<Type> GetExpectedParametersType(MethodBase targetMethod)
        {
            var expectedParametersType = new List<Type>();
            var expectInstanceParameter = !targetMethod.IsStatic && !targetMethod.IsConstructor;

            if (expectInstanceParameter)
                expectedParametersType.Add(targetMethod.DeclaringType);

            expectedParametersType.AddRange(targetMethod.GetParameters().Select(p => p.ParameterType));
            return expectedParametersType;
        }

        private static void ThrowInvalidHookSignature(MethodInfo hookMethod,
            IEnumerable<Tuple<string, Type>> expectedParams, Type expectedReturnType)
        {
            var expectedParamsArr = expectedParams
                .Select(p => $"{p.Item2} {p.Item1}")
                .ToList();

            if (expectedReturnType != typeof(void))
                expectedParamsArr.Add($"{expectedReturnType} result");

            var expectedSignature = $"{typeof(void)} HookMethod({string.Join(", ", expectedParamsArr)})";

            var actualParamsArr = hookMethod.GetParameters().Select(p => $"{p.ParameterType} {p.Name}").ToArray();
            var actualSignature = $"{hookMethod.ReturnType} {hookMethod.Name}({string.Join(",", actualParamsArr)})";

            throw new ArgumentException(
                $"Invalid method signature '{actualSignature}'. Expected: '{expectedSignature}'");
        }
    }
}