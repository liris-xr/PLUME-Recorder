using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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
                
                // A module whose constructor fails is left out: the others still record.
                try
                {
                    modules.Add((IRecorderModule)Activator.CreateInstance(moduleType));
                }
                catch (Exception e)
                {
                    Logger.LogWarning($"Failed to create the recorder module {moduleType}: {e.InnerException ?? e}. Skipping.");
                }
            }

            return modules.ToArray();
        }

        internal static IEnumerable<Type> GetRecorderModulesTypesFromAllAssemblies()
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var currentAssemblyName = currentAssembly.GetName();
            var referencingAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => asm.GetReferencedAssemblies().Any(asmRef => asmRef.FullName.Equals(currentAssemblyName.FullName)));
            
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
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                // Types depending on a missing assembly cannot be modules anyway.
                types = e.Types.Where(type => type != null).ToArray();
            }

            return types.Where(type => type.GetInterfaces().Contains(typeof(IRecorderModule)) &&
                                       !type.IsAbstract && !type.IsInterface);
        }
    }
}