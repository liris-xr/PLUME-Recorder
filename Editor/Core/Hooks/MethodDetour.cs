using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace PLUME.Editor.Core.Hooks
{
    public readonly struct MethodDetour
    {
        public readonly string Name;
        public readonly MethodBase TargetMethod;
        public readonly MethodInfo HookMethod;

        public MethodDetour(string name, MethodBase targetMethod, MethodInfo hookMethod)
        {
            Name = name;
            TargetMethod = targetMethod;
            HookMethod = hookMethod;
        }

        public bool TryInjectAt(Instruction instruction, ILProcessor processor)
        {
            var module = processor.Body.Method.Module;

            if (!InstructionMatchesTarget(module, instruction))
                return false;

            var hookMethodRef = module.ImportReference(HookMethod);
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

        private bool InstructionMatchesTarget(ModuleDefinition module, Instruction instruction)
        {
            if (instruction.OpCode != OpCodes.Call &&
                instruction.OpCode != OpCodes.Callvirt &&
                instruction.OpCode != OpCodes.Newobj)
                return false;

            if (instruction.Operand is not MethodReference instrMethodRef)
                return false;

            if (instrMethodRef.Name != TargetMethod.Name)
                return false;

            if (instrMethodRef.DeclaringType?.Name != TargetMethod.DeclaringType?.Name)
                return false;

            if (instrMethodRef.DeclaringType?.Namespace != TargetMethod.DeclaringType?.Namespace)
                return false;

            if (instrMethodRef is not GenericInstanceMethod && TargetMethod.IsGenericMethod)
                return false;

            if (instrMethodRef.Parameters.Count != TargetMethod.GetParameters().Length)
                return false;

            for (var i = 0; i < instrMethodRef.Parameters.Count; i++)
            {
                var parameter = instrMethodRef.Parameters[i];
                var expectedParameter = TargetMethod.GetParameters()[i];

                if (expectedParameter.ParameterType.IsGenericType)
                {
                    var parameterTypeDef = parameter.ParameterType.Resolve();
                    var expectedParamTypeDef = module.ImportReference(expectedParameter.ParameterType.GetGenericTypeDefinition()).Resolve();
                    
                    if (!parameterTypeDef.IsAssignableFrom(expectedParamTypeDef))
                        return false;
                }
                else
                {
                    var parameterTypeDef = parameter.ParameterType.Resolve();
                    var expectedParamTypeDef = module.ImportReference(expectedParameter.ParameterType).Resolve();

                    if (!expectedParamTypeDef.IsAssignableFrom(parameterTypeDef))
                        return false;
                }
            }

            if (instrMethodRef is not GenericInstanceMethod genericMethodRef)
                return true;

            var targetMethodGenericArgs = TargetMethod.GetGenericArguments();

            if (genericMethodRef.GenericArguments.Count != targetMethodGenericArgs.Length)
            {
                return false;
            }

            for (var i = 0; i < genericMethodRef.GenericArguments.Count; i++)
            {
                var argument = genericMethodRef.GenericArguments[i];
                
                // TODO: cast as GenericParameter and check constraints
                var argumentTypeDef = argument.Resolve();
                
                var expectedArgument = targetMethodGenericArgs[i];
                
                if (expectedArgument.IsGenericParameter)
                {
                    var constraints = expectedArgument.GetGenericParameterConstraints();
                    
                    foreach (var constraint in constraints)
                    {
                        TypeDefinition constraintTypeDef;
                        
                        if (constraint.IsGenericType)
                            constraintTypeDef = module.ImportReference(constraint.GetGenericTypeDefinition()).Resolve();
                        else
                            constraintTypeDef = module.ImportReference(constraint).Resolve();

                        if (!constraintTypeDef.IsAssignableFrom(argumentTypeDef))
                            return false;
                    }
                }
                else
                {
                    var parameterTypeDef = argument.Resolve();
                    var expectedParamTypeDef = module.ImportReference(expectedArgument).Resolve();

                    if (!parameterTypeDef.IsAssignableFrom(expectedParamTypeDef))
                        return false;
                }
            }

            return true;
        }
    }
}