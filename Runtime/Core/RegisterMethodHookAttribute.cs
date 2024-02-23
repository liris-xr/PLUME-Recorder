using System;
using System.Reflection;

namespace PLUME.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class RegisterHookAttribute : Attribute
    {
        public MethodBase TargetMethod { get; }

        protected RegisterHookAttribute(MethodBase targetMethod)
        {
            TargetMethod = targetMethod;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterPropertySetterHookAttribute : RegisterHookAttribute
    {
        public RegisterPropertySetterHookAttribute(Type declaringType, string propertyName) : base(
            declaringType.GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetSetMethod())
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterPropertyGetterHookAttribute : RegisterHookAttribute
    {
        public RegisterPropertyGetterHookAttribute(Type declaringType, string propertyName) : base(
            declaringType.GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetGetMethod())
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterConstructorHookAttribute : RegisterHookAttribute
    {
        public RegisterConstructorHookAttribute(Type declaringType) : base(
            declaringType.GetConstructor(Type.EmptyTypes))
        {
        }

        public RegisterConstructorHookAttribute(Type declaringType, params Type[] types) : base(
            declaringType.GetConstructor(types))
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterMethodHookAttribute : RegisterHookAttribute
    {
        /// <summary>
        /// Register a hook for a method.
        /// </summary>
        /// <param name="declaringType"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        public RegisterMethodHookAttribute(Type declaringType, string methodName, params Type[] parameters) : base(
            declaringType.GetMethod(methodName, parameters))
        {
        }

        /// <summary>
        /// Register a hook for a method.
        /// </summary>
        /// <param name="targetMethod"></param>
        public RegisterMethodHookAttribute(MethodInfo targetMethod) : base(targetMethod)
        {
        }
    }
}