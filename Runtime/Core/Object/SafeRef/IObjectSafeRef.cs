using System;
using UnityObject = UnityEngine.Object;

namespace PLUME.Core.Object.SafeRef
{
    public interface IObjectSafeRef
    {
        Type ObjectType { get; }

        ObjectIdentifier Identifier { get; }

        UnityObject Object { get; }

        string ToString();
    }
}