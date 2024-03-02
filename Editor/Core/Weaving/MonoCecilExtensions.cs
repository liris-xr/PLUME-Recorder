using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using UnityEngine;

namespace PLUME.Editor.Core.Events
{
    internal static class MonoCecilExtensions
    {
        public static IEnumerable<MethodDefinition> GetAllMethods(this AssemblyDefinition assemblyDefinition)
        {
            var types = assemblyDefinition.Modules.SelectMany(m => CollectTypesRecursively(m.Types));
            return types.SelectMany(t => t.Methods).Distinct();
        }

        private static IEnumerable<TypeDefinition> CollectTypesRecursively(IEnumerable<TypeDefinition> types)
        {
            var collectedTypes = new List<TypeDefinition>();

            foreach (var typeDefinition in types)
            {
                collectedTypes.Add(typeDefinition);

                if (typeDefinition.HasNestedTypes)
                {
                    collectedTypes.AddRange(CollectTypesRecursively(typeDefinition.NestedTypes));
                }
            }

            return collectedTypes;
        }

        /// <summary>
        /// Is childTypeDef a subclass of parentTypeDef. Does not test interface inheritance
        /// </summary>
        /// <param name="childTypeDef"></param>
        /// <param name="parentTypeDef"></param>
        /// <returns></returns>
        public static bool IsSubclassOf(this TypeDefinition childTypeDef, TypeDefinition parentTypeDef) =>
            childTypeDef.MetadataToken != parentTypeDef.MetadataToken
            && EnumerateBaseClasses(childTypeDef).Any(b => Equals(b, parentTypeDef));

        /// <summary>
        /// Does childType inherit from parentInterface
        /// </summary>
        /// <param name="childType"></param>
        /// <param name="parentInterfaceDef"></param>
        /// <returns></returns>
        public static bool DoesAnySubTypeImplementInterface(this TypeDefinition childType,
            TypeDefinition parentInterfaceDef)
        {
            Debug.Assert(parentInterfaceDef.IsInterface);

            return
                EnumerateBaseClasses(childType)
                    .Any(typeDefinition => DoesSpecificTypeImplementInterface(typeDefinition, parentInterfaceDef));
        }

        /// <summary>
        /// Does the childType directly inherit from parentInterface. Base
        /// classes of childType are not tested
        /// </summary>
        /// <param name="childTypeDef"></param>
        /// <param name="parentInterfaceDef"></param>
        /// <returns></returns>
        public static bool DoesSpecificTypeImplementInterface(this TypeDefinition childTypeDef,
            TypeDefinition parentInterfaceDef)
        {
            Debug.Assert(parentInterfaceDef.IsInterface);
            return childTypeDef
                .Interfaces
                .Any(ifaceDef =>
                    DoesSpecificInterfaceImplementInterface(ifaceDef.InterfaceType.Resolve(), parentInterfaceDef));
        }

        /// <summary>
        /// Does interface iface0 equal or implement interface iface1
        /// </summary>
        /// <param name="iface0"></param>
        /// <param name="iface1"></param>
        /// <returns></returns>
        public static bool DoesSpecificInterfaceImplementInterface(TypeDefinition iface0, TypeDefinition iface1)
        {
            Debug.Assert(iface1.IsInterface);
            Debug.Assert(iface0.IsInterface);
            return Equals(iface0, iface1) || DoesAnySubTypeImplementInterface(iface0, iface1);
        }

        /// <summary>
        /// Is source type assignable to target type
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsAssignableFrom(this TypeDefinition target, TypeDefinition source)
            => target == source
               || Equals(target, source)
               || IsSubclassOf(source, target)
               || target.IsInterface && DoesAnySubTypeImplementInterface(source, target);

        /// <summary>
        /// Enumerate the current type, it's parent and all the way to the top type
        /// </summary>
        /// <param name="classType"></param>
        /// <returns></returns>
        public static IEnumerable<TypeDefinition> EnumerateBaseClasses(this TypeDefinition classType)
        {
            for (var typeDefinition = classType;
                 typeDefinition != null;
                 typeDefinition = typeDefinition.BaseType?.Resolve())
            {
                yield return typeDefinition;
            }
        }

        public static bool Equals(TypeDefinition a, TypeDefinition b)
        {
            return
                a.MetadataToken == b.MetadataToken
                && a.FullName == b.FullName;
        }
    }
}