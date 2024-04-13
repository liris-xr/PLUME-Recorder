using Mono.Cecil;
using Mono.Cecil.Cil;
using PLUME.Core.Hooks;

namespace PLUME.Editor.Core.Hooks
{
    public readonly struct HookInjectionResult
    {
        /// <summary>
        /// The hook that was injected.
        /// </summary>
        public readonly Hook Hook;
        
        /// <summary>
        /// The method that contains the instruction where the hook was injected.
        /// </summary>
        public readonly MethodDefinition InstructionMethod;
        
        /// <summary>
        /// The instruction where the hook was injected.
        /// </summary>
        public readonly Instruction Instruction;

        public HookInjectionResult(Hook hook, MethodDefinition instructionMethod, Instruction instruction)
        {
            Hook = hook;
            InstructionMethod = instructionMethod;
            Instruction = instruction;
        }
    }
}