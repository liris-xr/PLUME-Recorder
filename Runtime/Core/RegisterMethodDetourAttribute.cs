using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PLUME.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterMethodDetourAttribute : Attribute
    {
        public MethodBase TargetMethod { get; }

        protected RegisterMethodDetourAttribute(MethodBase targetMethod)
        {
            TargetMethod = targetMethod;
        }

        /// <summary>
        /// Register a hook for a method.
        /// </summary>
        /// <param name="declaringType"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        public RegisterMethodDetourAttribute(Type declaringType, string methodName, params Type[] parameters)
        {
            var methods = GetMethods(declaringType, methodName, parameters);

            if (methods.Count == 0)
                throw new ArgumentException(
                    $"No method found with name {methodName} and parameters '{string.Join(", ", parameters.Select(p => p.Name))}' in type {declaringType.Name}");
            if (methods.Count > 1)
                throw new ArgumentException(
                    $"Multiple methods found with name {methodName} and parameters '{string.Join(", ", parameters.Select(p => p.Name))}' in type {declaringType.Name}");

            TargetMethod = methods.First();
        }

        /// <summary>
        /// Get a method by name and parameters. This method is useful to include generic parameters during the search
        /// as the GetMethod method does not support them.
        /// </summary>
        /// <param name="declaringType"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static List<MethodBase> GetMethods(Type declaringType, string methodName,
            IReadOnlyList<Type> parameters)
        {
            var methods = declaringType.GetMethods();
            var candidates = new List<MethodBase>();

            foreach (var method in methods.Where(m => m.Name == methodName))
            {
                var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

                if (methodParameterTypes.Length != parameters.Count)
                    continue;

                var validParameters = true;

                for (var i = 0; i < parameters.Count; ++i)
                {
                    var x = parameters[i];
                    var y = methodParameterTypes[i];
                    if (x.Assembly == y.Assembly &&
                        x.Namespace == y.Namespace &&
                        x.Name == y.Name) continue;
                    validParameters = false;
                    break;
                }

                if (validParameters)
                    candidates.Add(method);
            }

            return candidates;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterConstructorDetourAttribute : RegisterMethodDetourAttribute
    {
        public RegisterConstructorDetourAttribute(Type declaringType, params Type[] parameters) : base(
            declaringType.GetConstructor(parameters))
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterPropertySetterDetourAttribute : RegisterMethodDetourAttribute
    {
        public RegisterPropertySetterDetourAttribute(Type declaringType, string propertyName) : base(
            declaringType.GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetSetMethod())
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterPropertyGetterDetourAttribute : RegisterMethodDetourAttribute
    {
        public RegisterPropertyGetterDetourAttribute(Type declaringType, string propertyName) : base(
            declaringType.GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetGetMethod())
        {
        }
    }
}