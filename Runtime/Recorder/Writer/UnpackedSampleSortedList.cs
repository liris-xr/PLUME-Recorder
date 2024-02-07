using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PLUME.Sample;

namespace PLUME.Recorder.Writer
{
    public class UnpackedSampleSortedList : IEnumerable
    {
        private readonly List<UnpackedSample> _list = new();
        private readonly IComparer<UnpackedSample> _comparer = new UnpackedSampleComparer();
        private object _lock = new();

        public IEnumerator<UnpackedSample> GetEnumerator()
        {
            lock (_lock)
            {
                return _list.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_lock)
            {
                return _list.GetEnumerator();
            }
        }

        public void CopyTo(Array array, int index)
        {
            lock (_lock)
            {
                _list.CopyTo(array as UnpackedSample[] ?? Array.Empty<UnpackedSample>(), index);
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _list.Count;
                }
            }
        }

        public UnpackedSample Peek()
        {
            lock (_lock)
            {
                return _list.FirstOrDefault();
            }
        }

        public bool IsEmpty()
        {
            lock (_lock)
            {
                return _list.Count == 0;
            }
        }

        public bool Peek(out UnpackedSample sample)
        {
            lock (_lock)
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

        public void Add(UnpackedSample sample)
        {
            lock (_lock)
            {
                var idx = _list.BinarySearch(sample, _comparer);
                _list.Insert(idx >= 0 ? idx : ~idx, sample);
            }
        }

        public void AddRange(IEnumerable<UnpackedSample> samples)
        {
            lock (_lock)
            {
                foreach (var sample in samples)
                {
                    var idx = _list.BinarySearch(sample, _comparer);
                    _list.Insert(idx >= 0 ? idx : ~idx, sample);
                }
            }
        }

        public bool TryTake(out UnpackedSample sample)
        {
            lock (_lock)
            {
                if (_list.Count == 0)
                {
                    sample = default;
                    return false;
                }

                sample = _list.ElementAt(0);
                _list.RemoveAt(0);
                return true;
            }
        }
    }
}