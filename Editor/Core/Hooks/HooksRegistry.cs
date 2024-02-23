using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PLUME.Core;

namespace PLUME.Editor.Core.Hooks
{
    internal class HooksRegistry
    {
        private readonly List<MethodHook> _hooks = new();

        internal void Clear()
        {
            _hooks.Clear();
        }

        internal void RegisterHooks()
        {
            // Find all methods with the RegisterHookAttribute and register them
            // Get all assemblies referencing assembly PLUME.Recorder
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var runtimeAsm = assemblies.First(asm => asm.GetName().Name == "PLUME.Recorder");
            
            var registerHookAttributes = assemblies
                .Where(asm =>
                    asm == runtimeAsm || asm.GetReferencedAssemblies()
                        .Any(asmName => asmName.Name == runtimeAsm.GetName().Name))
                .SelectMany(asm => asm.GetTypes()).SelectMany(t =>
                    t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                .Select(m => new
                {
                    method = m, attribute = m.GetCustomAttributes<RegisterHookAttribute>().FirstOrDefault()
                })
                .Where(v => v.attribute != null);

            foreach (var result in registerHookAttributes)
            {
                if (!result.method.IsPublic)
                {
                    Logger.LogWarning($"Method {result.method} is not public. Skipping.");
                    continue;
                }

                if (result.attribute.TargetMethod == null)
                {
                    Logger.LogWarning($"TargetMethod is null for {result.method}. Skipping.");
                    continue;
                }

                RegisterHook(result.attribute.TargetMethod, result.method);
            }
        }

        private void RegisterHook(MethodBase targetMethod, MethodInfo hookMethod)
        {
            if (targetMethod == null)
                throw new ArgumentNullException(nameof(targetMethod));
            if (hookMethod == null)
                throw new ArgumentNullException(nameof(hookMethod));

            var hook = MethodHookBuilder.CreateHook(hookMethod.Name, targetMethod, hookMethod);
            _hooks.Add(hook);
            Logger.Log($"Registered hook {hookMethod} for {targetMethod}.");
        }

        internal IEnumerable<MethodHook> RegisteredHooks => _hooks;
    }
}