using System.Collections.Generic;
using PLUME;
using UnityEngine;

public class AndroidEventDispatcher : MonoBehaviour
{
    private readonly Dictionary<int, Object> _allObjectsById = new();
    private readonly List<int> _objectsToRemove = new();

    private void Start()
    {
        DispatchEvents();
    }

    private void Update()
    {
        DispatchEvents();
    }

    private void DispatchEvents()
    {
        var allGameObjects =
            FindObjectsByType(typeof(GameObject), FindObjectsInactive.Include, FindObjectsSortMode.None);
        var allComponents = FindObjectsByType(typeof(Component), FindObjectsInactive.Include, FindObjectsSortMode.None);

        _objectsToRemove.Clear();

        foreach (var objEntry in _allObjectsById)
        {
            if (objEntry.Value == null)
            {
                if (ObjectEvents.OnDestroy != null)
                    ObjectEvents.OnDestroy.Invoke(objEntry.Key);
                _objectsToRemove.Add(objEntry.Key);
            }
        }

        foreach (var objId in _objectsToRemove)
        {
            _allObjectsById.Remove(objId);
        }

        foreach (var obj in allGameObjects)
        {
            var id = obj.GetInstanceID();

            if (!_allObjectsById.ContainsKey(id))
            {
                if (ObjectEvents.OnCreate != null)
                    ObjectEvents.OnCreate.Invoke(obj);
                _allObjectsById.Add(id, obj);
            }
        }

        foreach (var component in allComponents)
        {
            var id = component.GetInstanceID();

            if (!_allObjectsById.ContainsKey(id))
            {
                if (ObjectEvents.OnCreate != null)
                    ObjectEvents.OnCreate.Invoke(component);
                _allObjectsById.Add(id, component);
            }
        }
    }
}