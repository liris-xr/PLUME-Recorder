using System;
using UnityEditorInternal;
using UnityEngine;

namespace PLUME.Weaver
{
    [Serializable]
    [CreateAssetMenu(fileName = "WeaverSettings", menuName = "PLUME/WeaverSettings")]
    public class WeaverSettings : ScriptableObject
    {
        [SerializeField] public AssemblyDefinitionAsset[] additionalAssemblies;
    }
}