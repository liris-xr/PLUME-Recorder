using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace PLUME.Editor.Core.Hooks
{
    public delegate void InjectHookDelegate(ILProcessor processor, Instruction instruction);

    public readonly struct MethodHook
    {
        public readonly string Name;
        public readonly MethodBase TargetMethod;
        private readonly InjectHookDelegate _injector;

        public MethodHook(string name, MethodBase targetMethod, InjectHookDelegate injector)
        {
            Name = name;
            TargetMethod = targetMethod;
            _injector = injector;
        }

        public bool TryInjectAtInstruction(ILProcessor processor, Instruction instruction)
        {
            if (!CanInjectAtInstruction(instruction))
                return false;

            _injector(processor, instruction);
            return true;
        }

        private bool CanInjectAtInstruction(Instruction instruction)
        {
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

                if (parameter.ParameterType is GenericParameter)
                {
                    var constraints = expectedParameter.ParameterType.GetGenericParameterConstraints();

                    if (!constraints.All(c => parameter.ParameterType.IsAssignableFrom(c)))
                        return false;
                }
                else if (!parameter.ParameterType.IsAssignableFrom(expectedParameter.ParameterType))
                {
                    return false;
                }
            }

            if (instrMethodRef is not GenericInstanceMethod genericMethodRef) return true;

            var targetMethodGenericArgs = TargetMethod.GetGenericArguments();

            if (genericMethodRef.GenericArguments.Count != targetMethodGenericArgs.Length)
            {
                return false;
            }

            for (var i = 0; i < genericMethodRef.GenericArguments.Count; i++)
            {
                var parameter = genericMethodRef.GenericArguments[i];
                var expectedParameter = targetMethodGenericArgs[i];

                var constraints = expectedParameter.GetGenericParameterConstraints();

                if (!constraints.All(c => parameter.IsAssignableFrom(c)))
                    return false;
            }

            return true;
        }
    }
}