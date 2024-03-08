using Mono.Cecil;
using Mono.Cecil.Cil;

namespace PLUME.Editor.Core.Events
{
    public readonly struct MethodDetourInjectionResult
    {
        public readonly MethodDetour Detour;
        public readonly MethodDefinition InstructionMethod;
        public readonly Instruction Instruction;

        public MethodDetourInjectionResult(MethodDetour detour, MethodDefinition instructionMethod, Instruction instruction)
        {
            Detour = detour;
            InstructionMethod = instructionMethod;
            Instruction = instruction;
        }
    }
}