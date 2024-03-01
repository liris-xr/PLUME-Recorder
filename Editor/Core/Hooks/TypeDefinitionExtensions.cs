using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using UnityEngine;

namespace PLUME.Editor.Core.Hooks
{
    // From https://stackoverflow.com/a/69450344
    internal static class TypeDefinitionExtensions
    {
        /// <summary>
        /// Is childTypeDef a subclass of parentTypeDef. Does not test interface inheritance
        /// </summary>
        /// <param name="childTypeDef"></param>
        /// <param name="parentTypeDef"></param>
        /// <returns></returns>
        public static bool IsSubclassOf(this TypeDefinition childTypeDef, TypeDefinition parentTypeDef) =>
            childTypeDef.MetadataToken != parentTypeDef.MetadataToken
            && childTypeDef.EnumerateBaseClasses().Any(b => Equals(b, parentTypeDef));

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
                childType
                    .EnumerateBaseClasses()
                    .Any(typeDefinition => typeDefinition.DoesSpecificTypeImplementInterface(parentInterfaceDef));
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
            return Equals(iface0, iface1) || iface0.DoesAnySubTypeImplementInterface(iface1);
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
               || source.IsSubclassOf(target)
               || target.IsInterface && source.DoesAnySubTypeImplementInterface(target);

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