using System;
using System.Linq;
using System.Reflection;

namespace PLUME.Core.Hooks
{
    public class InvalidHookMethodException : Exception
    {
        public InvalidHookMethodException(MethodInfo hookMethod, MethodBase targetMethod, string message) : base(
            $"Invalid hook signature '{GetMethodSignature(hookMethod)}' for target method '{GetMethodSignature(targetMethod)}'. {message}")
        {
        }

        private static string GetMethodSignature(MethodBase method)
        {
            switch (method)
            {
                case ConstructorInfo constructor:
                    return
                        $"{constructor.DeclaringType!.Name}({string.Join(", ", constructor.GetParameters().Select(p => p.ParameterType))}).ctor";
                case MethodInfo methodInfo:
                {
                    var methodParamsTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToList();
                    var hasThis = method.CallingConvention.HasFlag(CallingConventions.HasThis);

                    if (hasThis)
                    {
                        methodParamsTypes.Insert(0, method.DeclaringType);
                    }

                    var signature = $"{methodInfo.ReturnType} {method.Name}";

                    if (methodInfo.GetGenericArguments().Length > 0)
                    {
                        signature += $"<{string.Join<Type>(", ", methodInfo.GetGenericArguments())}>";
                    }

                    signature += $"({string.Join(", ", methodParamsTypes)})";
                    return signature;
                }
                    default:
                    return method.ToString();
            }
        }
    }
}