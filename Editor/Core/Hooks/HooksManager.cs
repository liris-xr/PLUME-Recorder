// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
// using PLUME.Core;
//
// namespace PLUME.Editor.Core.Hooks
// {
//     internal static class HooksManager
//     {
//         internal static List<MethodHook> GetRegisteredHooks()
//         {
//             var hooks = new List<MethodHook>();
//             
//             // Find all methods with the RegisterHookAttribute and register them
//             // Get all assemblies referencing assembly PLUME.Recorder
//             var assemblies = AppDomain.CurrentDomain.GetAssemblies();
//             var runtimeAsm = assemblies.First(asm => asm.GetName().Name == "PLUME.Recorder");
//
//             var registerHookAttributes = assemblies
//                 .Where(asm => asm == runtimeAsm || asm.GetReferencedAssemblies()
//                     .Any(asmName => asmName.Name == runtimeAsm.GetName().Name))
//                 .SelectMany(asm => asm.GetTypes())
//                 .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
//                 .Select(m => new
//                 {
//                     method = m, attribute = m.GetCustomAttributes<RegisterHookAttribute>().FirstOrDefault()
//                 })
//                 .Where(v => v.attribute != null);
//
//             foreach (var result in registerHookAttributes)
//             {
//                 if (!result.method.IsPublic)
//                 {
//                     Logger.LogWarning($"{result.method} is not public. Only public methods can be registered as hooks. Skipping hook.");
//                 }
//                 else if (result.attribute.TargetMethod == null)
//                 {
//                     Logger.LogWarning($"{result.method} target method not found. Skipping hook.");
//                 }
//                 else
//                 {
//                     if (result.attribute.TargetMethod == null)
//                         throw new ArgumentNullException(nameof(result.attribute.TargetMethod));
//                     if (result.method == null)
//                         throw new ArgumentNullException(nameof(result.method));
//
//                     CheckHookSignatureValidity(result.method, result.attribute.TargetMethod,
//                         result.attribute.InsertAfter);
//
//                     var hookName = result.method.DeclaringType?.Name + "." + result.method.Name;
//                     var hook = MethodHookBuilder.CreateHook(hookName, result.attribute.TargetMethod, result.method,
//                         result.attribute.InsertAfter);
//                     hooks.Add(hook);
//                 }
//             }
//             
//             return hooks;
//         }
//
//         private static void CheckHookSignatureValidity(MethodInfo hookMethod, MethodBase targetMethod, bool insertAfter)
//         {
//             if (hookMethod.ReturnType != typeof(void))
//                 throw new ArgumentException("Invalid return type for hook method. It should be 'void'");
//
//             var targetMethodReturnValue = targetMethod switch
//             {
//                 ConstructorInfo ctor => ctor.DeclaringType,
//                 MethodInfo methodInfo => methodInfo.ReturnType,
//                 _ => typeof(void)
//             };
//
//             var targetMethodParameters = new List<Tuple<string, Type>>();
//
//             if (!targetMethod.IsStatic && !targetMethod.IsConstructor)
//             {
//                 // Instance method
//                 targetMethodParameters.Add(new Tuple<string, Type>("instance", targetMethod.DeclaringType));
//             }
//
//             // Add parameters
//             targetMethodParameters.AddRange(targetMethod.GetParameters()
//                 .Select(p => new Tuple<string, Type>(p.Name, p.ParameterType)));
//
//             var hasReturnValue = targetMethodReturnValue != typeof(void);
//             var expectedCount = targetMethodParameters.Count + (hasReturnValue ? 1 : 0);
//
//             var hookParameters = hookMethod.GetParameters();
//
//             if (expectedCount != hookParameters.Length)
//                 ThrowInvalidHookSignature(hookMethod, targetMethodParameters, targetMethodReturnValue);
//
//             // Last parameter should be the return value
//             for (var i = 0; i < hookParameters.Length; i++)
//             {
//                 var hookParameter = hookParameters[i].ParameterType;
//
//                 var expectedType = i == hookParameters.Length - 1 && hasReturnValue
//                     ? targetMethodReturnValue
//                     : targetMethodParameters[i].Item2;
//
//                 if (!hookParameter.IsAssignableFrom(expectedType))
//                     ThrowInvalidHookSignature(hookMethod, targetMethodParameters, targetMethodReturnValue);
//             }
//         }
//
//         private static void ThrowInvalidHookSignature(MethodInfo hookMethod,
//             IEnumerable<Tuple<string, Type>> expectedParams, Type expectedReturnType)
//         {
//             var expectedParamsArr = expectedParams
//                 .Select(p => $"{p.Item2} {p.Item1}")
//                 .ToList();
//
//             if (expectedReturnType != typeof(void))
//                 expectedParamsArr.Add($"{expectedReturnType} result");
//
//             var expectedSignature = $"{typeof(void)} HookMethod({string.Join(", ", expectedParamsArr)})";
//
//             var actualParamsArr = hookMethod.GetParameters().Select(p => $"{p.ParameterType} {p.Name}").ToArray();
//             var actualSignature = $"{hookMethod.ReturnType} {hookMethod.Name}({string.Join(",", actualParamsArr)})";
//
//             throw new ArgumentException(
//                 $"Invalid method signature '{actualSignature}'. Expected: '{expectedSignature}'");
//         }
//     }
// }