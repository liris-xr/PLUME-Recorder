using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PLUME.Sample;

namespace PLUME
{
    internal struct PackedSampleKey : IComparable<PackedSampleKey>
    {
        public ulong Timestamp;
        public ulong Seq;

        public int CompareTo(PackedSampleKey other)
        {
            var timestampComparison = Timestamp.CompareTo(other.Timestamp);
            return timestampComparison != 0 ? timestampComparison : Seq.CompareTo(other.Seq);
        }
    }
    
    public class OrderedPackedSampleList : IProducerConsumerCollection<PackedSample>
    {
        private readonly SortedList<PackedSampleKey, PackedSample> _sortedList = new();
        
        public IEnumerator<PackedSample> GetEnumerator()
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
                _sortedList.Values.CopyTo(array as PackedSample[] ?? Array.Empty<PackedSample>(), index);
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

        public bool IsSynchronized => true;
        public object SyncRoot { get; } = new();

        public void CopyTo(PackedSample[] array, int index)
        {
            lock (SyncRoot)
            {
                _sortedList.Values.CopyTo(array, index);
            }
        }

        public PackedSample[] ToArray()
        {
            lock (SyncRoot)
            {
                return _sortedList.Values.ToArray();
            }
        }

        public PackedSample Peek()
        {
            lock (SyncRoot)
            {
                return _sortedList.Count == 0 ? null : _sortedList.FirstOrDefault().Value;
            }
        }

        public bool IsEmpty()
        {
            lock (SyncRoot)
            {
                return _sortedList.Count == 0;
            }
        }
        
        public bool Peek(out PackedSample sample)
        {
            lock (SyncRoot)
            {
                if (_sortedList.Count == 0)
                {
                    sample = null;
                    return false;
                }
                sample = _sortedList.First().Value;
                return true;
            }
        }
        
        public bool TryAdd(PackedSample item)
        {
            lock (SyncRoot)
            {
                _sortedList.Add(new PackedSampleKey {Timestamp = item.Header.Time, Seq = item.Header.Seq}, item);
                return true;
            }
        }

        public bool TryTake(out PackedSample item)
        {
            lock (SyncRoot)
            {
                if (_sortedList.Count == 0)
                {
                    item = null;
                    return false;
                }

                item = _sortedList.Values[0];
                _sortedList.RemoveAt(0);
                return true;
            }
        }
    }
}