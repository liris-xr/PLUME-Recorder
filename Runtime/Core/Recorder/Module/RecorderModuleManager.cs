using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PLUME.Core.Recorder.Module
{
    internal static class RecorderModuleManager
    {
        internal static IRecorderModule[] InstantiateRecorderModulesFromAllAssemblies()
        {
            var recorderModuleTypes = GetRecorderModulesTypesFromAllAssemblies();
            var recorderModules = InstantiateRecorderModulesFromTypes(recorderModuleTypes);
            return recorderModules;
        }

        internal static IRecorderModule[] InstantiateRecorderModulesFromTypes(IEnumerable<Type> moduleTypes)
        {
            var modules = new List<IRecorderModule>();

            foreach (var moduleType in moduleTypes)
            {
                if (!moduleType.GetInterfaces().Contains(typeof(IRecorderModule)))
                {
                    Logger.LogWarning($"Type {moduleType} does not implement {nameof(IRecorderModule)}. Skipping.");
                    continue;
                }

                if (moduleType.IsAbstract || moduleType.IsInterface)
                {
                    Logger.LogWarning($"Type {moduleType} is not a concrete class. Skipping.");
                    continue;
                }

                var parameterlessConstructor = moduleType.GetConstructor(Type.EmptyTypes);

                if (parameterlessConstructor == null)
                {
                    Logger.LogWarning($"Type {moduleType} does not have a default constructor. Skipping.");
                    continue;
                }
                
                var recorderModule = (IRecorderModule)Activator.CreateInstance(moduleType);
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