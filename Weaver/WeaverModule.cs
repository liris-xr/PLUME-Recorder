using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace PLUME.Weaver
{
    public abstract class WeaverModule
    {
        private AssemblyDefinition _assemblyDefinition;
        private TypeReference _monoBehaviourType;

        internal void VisitAssembly(AssemblyDefinition assemblyDefinition)
        {
            _assemblyDefinition = assemblyDefinition;
            _monoBehaviourType = assemblyDefinition.MainModule.ImportReference(typeof(MonoBehaviour));
            
            var mainModule = assemblyDefinition.MainModule;
            var classesDefs = mainModule.Types.Where(t => t.IsClass);

            foreach (var classDef in classesDefs)
            {
                foreach (var method in classDef.Methods)
                {
                    if(!method.HasBody)
                        continue;
                
                    var body = method.Body;
                    var instructions = body.Instructions;

                    foreach (var instruction in instructions)
                    {
                        if (instruction.OpCode.Code != Code.Newobj)
                            continue;
                    
                        if(instruction.Operand is not MethodReference methodReference)
                            continue;
                        
                        if (methodReference.DeclaringType.FullName != "UnityEngine.GameObject")
                            continue;
                        
                        Debug.Log(instruction.Operand);
                    }
                }
            }
            
            // foreach (var type in assemblyDefinition.MainModule.Types)
            // {
            //     var typeRef = assemblyDefinition.MainModule.ImportReference(type);
            //     
            //     Debug.Log(type.FullName);
            //     Debug.Log(typeRef.FullName);
            // }

            // foreach (var assemblyModule in assemblyDefinition.Modules)
            // {
            //     foreach (var type in assemblyModule.Types)
            //     {
            //         Debug.Log(type.Name);
            //     }
            // }

            // OnVisitModule(moduleDefinition);
            //
            // var types = FlattenTypes(moduleDefinition.Types).Where(t => t.Namespace != "UnityEngine");
            //
            // foreach (var type in types)
            // {
            //     var hasCoreRef = type.Module.ModuleReferences.Any(mRef => mRef.Name == "UnityEngine.CoreModule");
            //     var module = type.Module;
            //
            //     foreach (var moduleRef in module.ModuleReferences)
            //     {
            //         Debug.Log(type.Name + " has module ref: " + moduleRef.Name);
            //     }
            //     
            //     foreach (var asmRef in module.AssemblyReferences)
            //     {
            //         Debug.Log(type.Name + " has asm ref: " + asmRef.Name);
            //     }
            // }

            // if (moduleDefinition.AssemblyReferences.Any(m => m.Name == "UnityEngine.CoreModule"))
            // {
            //     VisitTypes(moduleDefinition.Types);
            // }
        }

        private List<TypeDefinition> FlattenTypes(IEnumerable<TypeDefinition> types)
        {
            var flattenedTypes = new List<TypeDefinition>();

            foreach (var type in types)
            {
                flattenedTypes.Add(type);

                if (type.HasNestedTypes)
                    flattenedTypes.AddRange(FlattenTypes(type.NestedTypes));
            }

            return flattenedTypes;
        }

        public bool IsMonoBehaviour(TypeDefinition type)
        {
            var currentType = type.GetElementType();

            do
            {
                // Check if current type is a MonoBehaviour or a subclass of MonoBehaviour
                if (currentType.FullName == _monoBehaviourType.FullName)
                {
                    return true;
                }

                currentType = currentType.Resolve().BaseType;
            } while (currentType != null);

            return false;
        }

        internal void VisitTypes(IEnumerable<TypeDefinition> types)
        {
            foreach (var type in types)
            {
                if (!type.IsClass || !IsMonoBehaviour(type))
                {
                    continue;
                }

                OnVisitType(type);

                foreach (var method in type.Methods)
                {
                    OnVisitMethod(method);
                }

                foreach (var field in type.Fields)
                {
                    OnVisitField(field);
                }

                foreach (var property in type.Properties)
                {
                    OnVisitProperty(property);
                }
            }
        }

        public abstract bool ShouldVisitTypes(ModuleDefinition moduleDefinition);

        public abstract bool ShouldVisitMethods(ModuleDefinition moduleDefinition);

        public abstract bool ShouldVisitFields(ModuleDefinition moduleDefinition);

        protected virtual void OnVisitModule(ModuleDefinition moduleDefinition)
        {
        }

        protected virtual void OnVisitType(TypeDefinition typeDefinition)
        {
        }

        protected virtual void OnVisitMethod(MethodDefinition methodDefinition)
        {
        }

        protected virtual void OnVisitField(FieldDefinition fieldDefinition)
        {
        }

        protected virtual void OnVisitProperty(PropertyDefinition propertyDefinition)
        {
        }
    }
}