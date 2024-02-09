using System;
using UnityObject = UnityEngine.Object;

namespace PLUME.Core.Object.SafeRef
{
    public interface IObjectSafeRef
    {
        Type GetObjectType();

        ObjectIdentifier GetObjectIdentifier();

        UnityObject GetObject();

        string ToString();
    }
}