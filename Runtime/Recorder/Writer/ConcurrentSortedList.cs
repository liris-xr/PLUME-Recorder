using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PLUME
{
    public class ConcurrentSortedList<TK, TV> : IEnumerable
    {
        public bool IsSynchronized => true;
        public object SyncRoot { get; } = new();
        private readonly SortedList<TK, TV> _sortedList = new();

        public IEnumerator<TV> GetEnumerator()
        {
            lock (SyncRoot)
            {
                return _sortedList.Values.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (SyncRoot)
            {
                return _sortedList.Values.GetEnumerator();
            }
        }

        public void CopyTo(Array array, int index)
        {
            lock (SyncRoot)
            {
                _sortedList.Values.CopyTo(array as TV[] ?? Array.Empty<TV>(), index);
            }
        }

        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return _sortedList.Count;
                }
            }
        }

        public KeyValuePair<TK, TV>? Peek()
        {
            lock (SyncRoot)
            {
                return _sortedList.Count == 0 ? null : _sortedList.FirstOrDefault();
            }
        }

        public bool IsEmpty()
        {
            lock (SyncRoot)
            {
                return _sortedList.Count == 0;
            }
        }

        public bool Peek(out KeyValuePair<TK, TV> sample)
        {
            lock (SyncRoot)
            {
                if (_sortedList.Count == 0)
                {
                    sample = default;
                    return false;
                }

                sample = _sortedList.First();
                return true;
            }
        }

        public void Add(TK key, TV item)
        {
            lock (SyncRoot)
            {
                _sortedList.Add(key, item);
            }
        }

        public bool TryTake(out KeyValuePair<TK, TV> item)
        {
            lock (SyncRoot)
            {
                if (_sortedList.Count == 0)
                {
                    item = default;
                    return false;
                }

                item = _sortedList.ElementAt(0);
                _sortedList.RemoveAt(0);
                return true;
            }
        }
    }
}