using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PLUME.Guid
{
    [Serializable]
    public class GuidRegistry<T> : ISerializationCallbackReceiver where T : GuidRegistryEntry
    {
        [SerializeField]
        [SerializeReference]
        private List<T> entries = new();
        
        [NonSerialized]
        private Dictionary<Object, T> _guids = new();

        public int Count => _guids.Count;

        public void Clear()
        {
            _guids.Clear();
        }

        public bool TryAdd(T guidEntry)
        {
            return _guids.TryAdd(guidEntry.@object, guidEntry);
        }

        public Dictionary<Object, T> Copy()
        {
            return new Dictionary<Object, T>(_guids);
        }

        public bool TryGetValue(Object obj, out T entry)
        {
            return _guids.TryGetValue(obj, out entry);
        }
        
        public void OnBeforeSerialize()
        {
            entries ??= new List<T>();
            entries.Clear();
            foreach (var (obj, entry) in _guids)
            {
                if(obj != null)
                    entries.Add(entry);
            }
        }

        public void OnAfterDeserialize()
        {
            _guids ??= new Dictionary<Object, T>();
            _guids.Clear();
            foreach (var entry in entries.Where(entry => entry != null && entry.@object != null))
            {
                if (entry.@object != null)
                {
                    try
                    {
                        _guids.Add(entry.@object, entry);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }
    }
}