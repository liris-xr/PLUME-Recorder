using Mono.Cecil.Cil;

namespace PLUME.Editor.Core.Hooks
{
    public readonly struct HooksInjectorResult
    {
        public readonly string HookName;
        public readonly string MethodName;
        public readonly Instruction Instruction;
        
        public HooksInjectorResult(string hookName, string methodName, Instruction instruction)
        {
            HookName = hookName;
            MethodName = methodName;
            Instruction = instruction;
        }
    }
}