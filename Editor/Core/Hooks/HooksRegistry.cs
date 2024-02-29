using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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

            var sb = new StringBuilder();
            sb.AppendLine("Hook registration results:");

            foreach (var result in registerHookAttributes)
            {
                if (!result.method.IsPublic)
                {
                    sb.AppendLine(
                        $"SKIPPED: {result.method} is not public. Only public methods can be registered as hooks.");
                }
                else if (result.attribute.TargetMethod == null)
                {
                    sb.AppendLine($"SKIPPED: {result.method} target method not found.");
                }
                else
                {
                    RegisterHook(result.attribute.TargetMethod, result.method, result.attribute.InsertAfter);
                    sb.AppendLine(
                        $"SUCCESS: {result.method} registered for target {result.attribute.TargetMethod.DeclaringType?.Name + "::" + result.attribute.TargetMethod}");
                }
            }

            Logger.Log(sb.ToString());
        }

        private void RegisterHook(MethodBase targetMethod, MethodInfo hookMethod, bool insertAfter = true)
        {
            if (targetMethod == null)
                throw new ArgumentNullException(nameof(targetMethod));
            if (hookMethod == null)
                throw new ArgumentNullException(nameof(hookMethod));

            var hook = MethodHookBuilder.CreateHook(hookMethod.DeclaringType?.Name + "." + hookMethod.Name,
                targetMethod, hookMethod, insertAfter);
            _hooks.Add(hook);
        }

        internal IEnumerable<MethodHook> RegisteredHooks => _hooks;
    }
}