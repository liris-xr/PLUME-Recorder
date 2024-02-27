using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace PLUME.Editor.Core.Hooks
{
    public static class MethodHookBuilder
    {
        public static MethodHook CreateHook(string name, MethodBase targetMethod, MethodInfo hookMethod,
            bool insertAfter = true)
        {
            CheckHookSignatureValidity(hookMethod, targetMethod, insertAfter);

            var targetMethodReturnValue = targetMethod switch
            {
                ConstructorInfo ctor => ctor.DeclaringType,
                MethodInfo methodInfo => methodInfo.ReturnType,
                _ => typeof(void)
            };

            var targetMethodParameters = new List<Type>();

            if (!targetMethod.IsStatic && !targetMethod.IsConstructor)
            {
                // Instance method
                targetMethodParameters.Add(targetMethod.DeclaringType);
            }

            // Add parameters
            targetMethodParameters.AddRange(targetMethod.GetParameters()
                .Select(parameter => parameter.ParameterType));

            return new MethodHook(name, targetMethod, (processor, instruction) =>
            {
                var module = processor.Body.Method.Module;
                var hookMethodRef = module.ImportReference(hookMethod);

                var newInstructions = new List<Instruction>();

                VariableDefinition returnValue = null;

                var targetMethodRef = instruction.Operand as MethodReference;

                // Copy the parameters to local variables
                var tmpVariables = new VariableDefinition[targetMethodParameters.Count];

                // Create local variables
                for (var i = 0; i < targetMethodParameters.Count; i++)
                {
                    var expectedParameter = targetMethodParameters[i];
                    var variable = new VariableDefinition(expectedParameter.AsTypeReference(module));
                    processor.Body.Variables.Add(variable);
                    tmpVariables[i] = variable;
                }

                // Copy the parameters to local variables
                foreach (var variable in tmpVariables.Reverse())
                {
                    newInstructions.Add(processor.Create(OpCodes.Stloc_S, variable));
                }

                // Push the parameter back onto the stack for the original method call or the hook call if inserted before
                foreach (var variable in tmpVariables)
                {
                    newInstructions.Add(processor.Create(OpCodes.Ldloc_S, variable));
                }

                if (insertAfter)
                {
                    // Call the original method
                    newInstructions.Add(processor.Create(instruction.OpCode, targetMethodRef));

                    // Store the result from the call if any
                    if (targetMethodReturnValue != typeof(void))
                    {
                        returnValue = new VariableDefinition(targetMethodReturnValue.AsTypeReference(module));
                        processor.Body.Variables.Add(returnValue);

                        // Duplicate the return value so that the hooked method can pop it for its own use
                        newInstructions.Add(processor.Create(OpCodes.Dup));
                        newInstructions.Add(processor.Create(OpCodes.Stloc_S, returnValue));
                    }

                    // Reload the parameters from the local variables for the hook call
                    foreach (var variable in tmpVariables)
                    {
                        newInstructions.Add(processor.Create(OpCodes.Ldloc_S, variable));
                    }

                    // Load the return value onto the stack
                    if (returnValue != null)
                    {
                        newInstructions.Add(processor.Create(OpCodes.Ldloc_S, returnValue));
                    }

                    // Call the hook
                    newInstructions.Add(processor.Create(OpCodes.Call, hookMethodRef));
                }
                else
                {
                    // Call the hook method
                    newInstructions.Add(processor.Create(OpCodes.Call, hookMethodRef));

                    // Reload the parameters from the local variables for the original call
                    foreach (var variable in tmpVariables)
                    {
                        newInstructions.Add(processor.Create(OpCodes.Ldloc_S, variable));
                    }

                    // Call the original method
                    newInstructions.Add(processor.Create(OpCodes.Call, targetMethodRef));
                }

                // Replace the instructions starting from the first parameter load
                processor.ReplaceWithRange(instruction, newInstructions);
            });
        }

        private static void CheckHookSignatureValidity(MethodInfo hookMethod, MethodBase targetMethod, bool insertAfter)
        {
            if (hookMethod.ReturnType != typeof(void))
                throw new ArgumentException("Invalid return type for hook method. It should be 'void'");

            var targetMethodReturnValue = targetMethod switch
            {
                ConstructorInfo ctor => ctor.DeclaringType,
                MethodInfo methodInfo => methodInfo.ReturnType,
                _ => typeof(void)
            };

            var targetMethodParameters = new List<Tuple<string, Type>>();

            if (!targetMethod.IsStatic && !targetMethod.IsConstructor)
            {
                // Instance method
                targetMethodParameters.Add(new Tuple<string, Type>("instance", targetMethod.DeclaringType));
            }

            // Add parameters
            targetMethodParameters.AddRange(targetMethod.GetParameters()
                .Select(p => new Tuple<string, Type>(p.Name, p.ParameterType)));

            var hasReturnValue = targetMethodReturnValue != typeof(void);
            var expectedCount = targetMethodParameters.Count + (hasReturnValue ? 1 : 0);

            var hookParameters = hookMethod.GetParameters();

            if (expectedCount != hookParameters.Length)
                ThrowInvalidHookSignature(hookMethod, targetMethodParameters, targetMethodReturnValue);

            // Last parameter should be the return value
            for (var i = 0; i < hookParameters.Length; i++)
            {
                var hookParameter = hookParameters[i].ParameterType;

                var expectedType = i == hookParameters.Length - 1 && hasReturnValue
                    ? targetMethodReturnValue
                    : targetMethodParameters[i].Item2;

                if (!hookParameter.IsAssignableFrom(expectedType))
                    ThrowInvalidHookSignature(hookMethod, targetMethodParameters, targetMethodReturnValue);
            }
        }

        private static void ThrowInvalidHookSignature(MethodInfo hookMethod,
            IEnumerable<Tuple<string, Type>> expectedParams, Type expectedReturnType)
        {
            var expectedParamsArr = expectedParams
                .Select(p => $"{p.Item2} {p.Item1}")
                .Append($"{expectedReturnType} result").ToArray();
            var expectedSignature = $"{typeof(void)} HookMethod({string.Join(",", expectedParamsArr)})";

            var actualParamsArr = hookMethod.GetParameters().Select(p => $"{p.ParameterType} {p.Name}").ToArray();
            var actualSignature = $"{hookMethod.ReturnType} {hookMethod.Name}({string.Join(",", actualParamsArr)})";

            throw new ArgumentException(
                $"Invalid method signature '{actualSignature}'. Expected: '{expectedSignature}'");
        }
    }
}