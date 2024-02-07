using System;
using UnityObject = UnityEngine.Object;

namespace PLUME
{
    public interface IObjectSafeRef
    {
        Type GetObjectType();

        ObjectIdentifier GetObjectIdentifier();

        UnityObject GetObject();

        string ToString();
    }
}