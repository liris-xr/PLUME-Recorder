using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace PLUME.Editor.Core.Hooks
{
    public static class ILProcessorExtensions
    {
        public static void ReplaceWithRange(this ILProcessor processor, Instruction target,
            IEnumerable<Instruction> instructions)
        {
            var first = true;
            var prevInstruction = target;

            foreach (var instruction in instructions)
            {
                if (first)
                {
                    processor.Replace(target, instruction);
                    first = false;
                }
                else
                {
                    processor.InsertAfter(prevInstruction, instruction);
                }

                prevInstruction = instruction;
            }
        }

        public static void InsertRangeBefore(this ILProcessor processor, Instruction target,
            IEnumerable<Instruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                processor.InsertBefore(target, instruction);
            }
        }

        public static void InsertRangeAfter(this ILProcessor processor, Instruction target,
            IEnumerable<Instruction> instructions)
        {
            var prevInstruction = target;

            foreach (var instruction in instructions)
            {
                processor.InsertAfter(prevInstruction, instruction);
                prevInstruction = instruction;
            }
        }
    }
}