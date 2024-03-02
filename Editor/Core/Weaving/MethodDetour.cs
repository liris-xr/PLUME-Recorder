using System.Reflection;

namespace PLUME.Editor.Core.Events
{
    public readonly struct MethodDetour
    {
        public readonly MethodBase TargetMethod;
        public readonly MethodInfo DetourMethod;

        public MethodDetour(MethodBase targetMethod, MethodInfo detourMethod)
        {
            TargetMethod = targetMethod;
            DetourMethod = detourMethod;
        }
    }
}