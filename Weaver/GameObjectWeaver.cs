using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Weaver
{
    [Preserve]
    public class GameObjectWeaver : WeaverModule
    {
        public override bool ShouldVisitTypes(ModuleDefinition moduleDefinition)
        {
            return false;
        }

        public override bool ShouldVisitMethods(ModuleDefinition moduleDefinition)
        {
            return moduleDefinition.AssemblyReferences.Any(m => m.Name == "UnityEngine.CoreModule");
        }

        public override bool ShouldVisitFields(ModuleDefinition moduleDefinition)
        {
            return false;
        }

        protected override void OnVisitMethod(MethodDefinition methodDefinition)
        {
            base.OnVisitMethod(methodDefinition);
            
            if (methodDefinition.Name != "Start")
                return;
            
            // Debug.Log("Visiting method: " + methodDefinition.Name);
            
            if (!methodDefinition.HasBody)
                return;

            var body = methodDefinition.Body;

            // ReSharper disable once ForCanBeConvertedToForeach
            for(var instructionIdx = 0; instructionIdx < body.Instructions.Count; ++instructionIdx)
            {
                var instruction = body.Instructions[instructionIdx];
                
                if (instruction.OpCode.Code != Code.Newobj)
                    continue;

                var methodReference = (MethodReference)instruction.Operand;
                if (methodReference.DeclaringType.FullName != "UnityEngine.GameObject")
                    continue;

                var debugLogMethodInfo = typeof(Debug).GetMethod("Log", new[] { typeof(string) });
                var debugLogMethodRef = methodDefinition.Module.ImportReference(debugLogMethodInfo);

                var processor = body.GetILProcessor();
                processor.InsertBefore(instruction, processor.Create(OpCodes.Ldstr, "GameObject created"));
                processor.InsertBefore(instruction, processor.Create(OpCodes.Call, debugLogMethodRef));
                
                Debug.Log("Weaved GameObject creation");
            }
        }
    }
}