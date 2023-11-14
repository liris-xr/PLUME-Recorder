using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PLUME
{
    public class ConcurrentSortedList<T> : IEnumerable
    {
        public bool IsSynchronized => true;
        public object SyncRoot { get; } = new();
        private readonly List<T> _list = new();
        private readonly IComparer<T> _comparer;

        public ConcurrentSortedList()
        {
        }

        public ConcurrentSortedList(IComparer<T> comparer)
        {
            _comparer = comparer;
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            lock (SyncRoot)
            {
                return _list.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (SyncRoot)
            {
                return _list.GetEnumerator();
            }
        }

        public void CopyTo(Array array, int index)
        {
            lock (SyncRoot)
            {
                _list.CopyTo(array as T[] ?? Array.Empty<T>(), index);
            }
        }

        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return _list.Count;
                }
            }
        }

        public T Peek()
        {
            lock (SyncRoot)
            {
                return _list.FirstOrDefault();
            }
        }

        public bool IsEmpty()
        {
            lock (SyncRoot)
            {
                return _list.Count == 0;
            }
        }

        public bool Peek(out T sample)
        {
            lock (SyncRoot)
            {
                if (_list.Count == 0)
                {
                    sample = default;
                    return false;
                }

                sample = _list.First();
                return true;
            }
        }

        public void Add(T item)
        {
            lock (SyncRoot)
            {
                var idx = _comparer == null ? _list.BinarySearch(item) : _list.BinarySearch(item, _comparer);
                _list.Insert(idx >= 0 ? idx : ~idx, item);
            }
        }

        public bool TryTake(out T item)
        {
            lock (SyncRoot)
            {
                if (_list.Count == 0)
                {
                    item = default;
                    return false;
                }

                item = _list.ElementAt(0);
                _list.RemoveAt(0);
                return true;
            }
        }
    }
}