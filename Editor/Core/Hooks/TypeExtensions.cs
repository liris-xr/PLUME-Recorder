using System;
using System.Linq;
using Mono.Cecil;

namespace PLUME.Editor.Core.Hooks
{
    public static class TypeExtensions
    {
        public static TypeReference AsTypeReference(this Type t, ModuleDefinition module)
        {
            if (!t.IsGenericType && !t.IsGenericTypeDefinition && !t.IsGenericParameter && !t.IsGenericMethodParameter)
            {
                return module.ImportReference(t);
            }
            
            return module.ImportReference(t.BaseType);
        }

        public static bool IsAssignableFrom(this TypeReference typeReference, Type t)
        {
            if (!t.ContainsGenericParameters)
                return typeReference.IsAssignableFrom(typeReference.Module.ImportReference(t));

            var constraints = t.GetGenericParameterConstraints();
            return constraints.All(typeReference.IsAssignableFrom);
        }

        public static bool IsAssignableFrom(this TypeReference typeReference, TypeReference t)
        {
            if (t == null)
                return false;
            if (typeReference == t)
                return true;

            var typeDefinition = typeReference.Resolve();

            if (typeReference is not GenericParameter genericParameter)
                return typeDefinition.IsInterface ? t.ImplementInterface(typeReference) : typeReference.IsSubclassOf(t);

            var constraints = genericParameter.Constraints;

            if (!constraints.All(c => t.IsAssignableFrom(c.ConstraintType)))
                return false;

            return typeDefinition.IsInterface ? t.ImplementInterface(typeReference) : typeReference.IsSubclassOf(t);
        }

        public static bool IsSubclassOf(this TypeReference typeReference, TypeReference c)
        {
            var typeDefinition = typeReference.Resolve();

            if (!typeDefinition.IsClass)
                return false;

            var targetTypeDefinition = c.Resolve();
            var currentTypeDefinition = typeDefinition;

            do
            {
                if (currentTypeDefinition == targetTypeDefinition)
                    return true;

                currentTypeDefinition = currentTypeDefinition.BaseType?.Resolve();
            } while (currentTypeDefinition != null);

            return false;
        }

        public static bool ImplementInterface(this TypeReference typeReference, TypeReference interfaceType)
        {
            var typeDef = typeReference.Resolve();
            return typeDef.Interfaces.Any(iter => iter.InterfaceType == interfaceType);
        }
    }
}