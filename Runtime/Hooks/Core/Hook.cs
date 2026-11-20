using System.Reflection;

namespace PLUME.Core.Hooks
{
    public readonly struct Hook
    {
        public readonly MethodInfo HookMethod;
        public readonly MethodBase TargetMethod;

        public Hook(MethodInfo hookMethod, MethodBase targetMethod)
        {
            HookMethod = hookMethod;
            TargetMethod = targetMethod;
        }
    }
}