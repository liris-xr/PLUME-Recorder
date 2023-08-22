using UnityEditor;
using UnityEngine;

//https://manuel-rauber.com/2022/05/23/instantiate-your-own-prefabs-via-gameobject-menu/
public class PlumePrefabManager : ScriptableObject
{
#if UNITY_EDITOR
    public GameObject recorderPrefab;
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(PlumePrefabManager))]
public class PlumePrefabManagerEditor : Editor
{
}
#endif