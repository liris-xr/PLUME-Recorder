using System;
using System.Linq;
using Mono.Cecil;
using UnityEngine;

namespace PLUME.Editor.Core.Hooks
{
    public static class TypeExtensions
    {
        // public static bool IsAssignableFrom(this TypeReference typeReference, Type t)
        // {
        //     if (t.IsGenericParameter)
        //     {
        //         var constraints = t.GetGenericParameterConstraints();
        //         return constraints.All(typeReference.IsAssignableFrom);
        //     }
        //
        //     if (t.ContainsGenericParameters)
        //     {
        //         foreach (var genericTypeArgument in t.GenericTypeArguments)
        //         {
        //             Debug.Log("Contains generic parameters");
        //         }
        //
        //         return true;
        //     }
        //
        //     return typeReference.IsAssignableFrom(typeReference.Module.ImportReference(t));
        // }
        //
        // public static bool IsAssignableFrom(this TypeReference typeReference, TypeReference t)
        // {
        //     if (t == null)
        //         return false;
        //     if (typeReference == t)
        //         return true;
        //
        //     if (typeReference is GenericParameter genericParameter)
        //     {
        //         var constraints = genericParameter.Constraints;
        //
        //         if (!constraints.All(c => t.IsAssignableFrom(c.ConstraintType)))
        //             return false;
        //
        //         return true;
        //     }
        //
        //     var typeDefinition = typeReference.Resolve();
        //     return typeDefinition.IsInterface ? t.ImplementInterface(typeReference) : typeReference.IsSubclassOf(t);
        // }
        //
        // public static bool IsSubclassOf(this TypeReference typeReference, TypeReference c)
        // {
        //     var typeDefinition = typeReference.Resolve();
        //
        //     if (!typeDefinition.IsClass)
        //         return false;
        //
        //     var targetTypeDefinition = c.Resolve();
        //     var currentTypeDefinition = typeDefinition;
        //
        //     do
        //     {
        //         if (currentTypeDefinition == targetTypeDefinition)
        //             return true;
        //
        //         currentTypeDefinition = currentTypeDefinition.BaseType?.Resolve();
        //     } while (currentTypeDefinition != null);
        //
        //     return false;
        // }
        //
        // public static bool ImplementInterface(this TypeReference typeReference, TypeReference interfaceType)
        // {
        //     var typeDef = typeReference.Resolve();
        //     return typeDef.Interfaces.Any(iter => iter.InterfaceType == interfaceType);
        // }
    }
}