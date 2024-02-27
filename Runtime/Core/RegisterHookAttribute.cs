using System;
using System.Reflection;

namespace PLUME.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class RegisterHookAttribute : Attribute
    {
        public MethodBase TargetMethod { get; }
        
        public bool InsertAfter { get; }

        protected RegisterHookAttribute(MethodBase targetMethod, bool insertAfter = true)
        {
            TargetMethod = targetMethod;
            InsertAfter = insertAfter;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterHookAfterPropertySetterAttribute : RegisterHookAttribute
    {
        public RegisterHookAfterPropertySetterAttribute(Type declaringType, string propertyName) : base(
            declaringType.GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetSetMethod())
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterHookAfterPropertyGetterAttribute : RegisterHookAttribute
    {
        public RegisterHookAfterPropertyGetterAttribute(Type declaringType, string propertyName) : base(
            declaringType.GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetGetMethod())
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterHookAfterConstructorAttribute : RegisterHookAttribute
    {
        public RegisterHookAfterConstructorAttribute(Type declaringType) : base(
            declaringType.GetConstructor(Type.EmptyTypes))
        {
        }

        public RegisterHookAfterConstructorAttribute(Type declaringType, params Type[] types) : base(
            declaringType.GetConstructor(types))
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterHookBeforeMethodAttribute : RegisterHookAttribute
    {
        /// <summary>
        /// Register a hook for a method.
        /// </summary>
        /// <param name="declaringType"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        public RegisterHookBeforeMethodAttribute(Type declaringType, string methodName, params Type[] parameters) : base(
            declaringType.GetMethod(methodName, parameters), false)
        {
        }

        /// <summary>
        /// Register a hook for a method.
        /// </summary>
        /// <param name="targetMethod"></param>
        public RegisterHookBeforeMethodAttribute(MethodInfo targetMethod) : base(targetMethod, false)
        {
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterHookAfterMethodAttribute : RegisterHookAttribute
    {
        /// <summary>
        /// Register a hook for a method.
        /// </summary>
        /// <param name="declaringType"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        public RegisterHookAfterMethodAttribute(Type declaringType, string methodName, params Type[] parameters) : base(
            declaringType.GetMethod(methodName, parameters))
        {
        }

        /// <summary>
        /// Register a hook for a method.
        /// </summary>
        /// <param name="targetMethod"></param>
        public RegisterHookAfterMethodAttribute(MethodInfo targetMethod) : base(targetMethod)
        {
        }
    }
}