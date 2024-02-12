using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PLUME.Core.Recorder.Module
{
    public static class RecorderModuleManager
    {
        internal static IRecorderModule[] InstantiateRecorderModulesFromTypes(IEnumerable<Type> moduleTypes)
        {
            var modules = new List<IRecorderModule>();

            foreach (var moduleType in moduleTypes)
            {
                if (!moduleType.GetInterfaces().Contains(typeof(IRecorderModule)))
                {
                    Debug.LogWarning($"Type {moduleType} does not implement {nameof(IRecorderModule)}. Skipping.");
                    continue;
                }

                if (moduleType.IsAbstract || moduleType.IsInterface)
                {
                    Debug.LogWarning($"Type {moduleType} is not a concrete class. Skipping.");
                    continue;
                }

                var parameterlessConstructor = moduleType.GetConstructor(Type.EmptyTypes);

                if (parameterlessConstructor == null)
                {
                    Debug.LogWarning($"Type {moduleType} does not have a default constructor. Skipping.");
                    continue;
                }
                
                var recorderModule = (IRecorderModule)Activator.CreateInstance(moduleType);
                recorderModule.Create();
                modules.Add(recorderModule);
            }

            return modules.ToArray();
        }

        internal static IEnumerable<Type> GetRecorderModulesTypesFromAllAssemblies()
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var currentAssemblyName = currentAssembly.GetName();
            var referencingAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => asm.GetReferencedAssemblies().Any(asmRef => asmRef.Equals(currentAssemblyName)));

            var moduleTypes = new List<Type>();

            moduleTypes.AddRange(GetRecorderModulesTypesFromAssembly(currentAssembly));

            foreach (var referencingAssembly in referencingAssemblies)
            {
                moduleTypes.AddRange(GetRecorderModulesTypesFromAssembly(referencingAssembly));
            }

            return moduleTypes.ToArray();
        }

        internal static IEnumerable<Type> GetRecorderModulesTypesFromAssembly(Assembly assembly)
        {
            return assembly.GetTypes().Where(type => type.GetInterfaces().Contains(typeof(IRecorderModule)) &&
                                                     !type.IsAbstract && !type.IsInterface);
        }
    }
}