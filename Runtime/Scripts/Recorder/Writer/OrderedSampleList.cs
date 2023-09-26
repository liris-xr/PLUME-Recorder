using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using PLUME.Sample;

namespace PLUME
{
    internal class SampleHeaderKey : IComparable<SampleHeaderKey>
    {
        public ulong Time;
        public ulong Seq;

        public int CompareTo(SampleHeaderKey other)
        {
            var timestampComparison = Time.CompareTo(other.Time);
            return timestampComparison != 0 ? timestampComparison : Seq.CompareTo(other.Seq);
        }
    }

    internal class SampleKey : IComparable<SampleKey>
    {
        public SampleHeaderKey Header;

        public int CompareTo(SampleKey other)
        {
            if (other.Header == null)
                return Header == null ? 0 : -1;
            if (Header == null)
                return 1;
            return Header.CompareTo(other.Header);
        }
    }

    public class OrderedSampleList : IProducerConsumerCollection<IMessage>
    {
        private readonly SortedList<SampleKey, IMessage> _sortedList = new();

        public IEnumerator<IMessage> GetEnumerator()
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
                _sortedList.Values.CopyTo(array as IMessage[] ?? Array.Empty<IMessage>(), index);
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

        public void CopyTo(IMessage[] array, int index)
        {
            lock (SyncRoot)
            {
                _sortedList.Values.CopyTo(array, index);
            }
        }

        public IMessage[] ToArray()
        {
            lock (SyncRoot)
            {
                return _sortedList.Values.ToArray();
            }
        }

        public IMessage Peek()
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

        public bool Peek(out IMessage sample)
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

        public bool TryAdd(IMessage item)
        {
            lock (SyncRoot)
            {
                if (item is SampleStamped sampleStamped)
                {
                    _sortedList.Add(new SampleKey
                    {
                        Header = new SampleHeaderKey
                        {
                            Time = sampleStamped.Header.Time,
                            Seq = sampleStamped.Header.Seq
                        }
                    }, item);
                }
                else
                {
                    _sortedList.Add(new SampleKey
                    {
                        Header = null
                    }, item);
                }

                return true;
            }
        }

        public bool TryTake(out IMessage item)
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