using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PLUME.Sample;

namespace PLUME
{
    public class UnpackedSampleSortedList : IEnumerable
    {
        private readonly List<UnpackedSample> _list = new();
        private readonly IComparer<UnpackedSample> _comparer = new UnpackedSampleComparer();

        public IEnumerator<UnpackedSample> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            _list.CopyTo(array as UnpackedSample[] ?? Array.Empty<UnpackedSample>(), index);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public UnpackedSample Peek()
        {
            return _list.FirstOrDefault();
        }

        public bool IsEmpty()
        {
            return _list.Count == 0;
        }

        public bool Peek(out UnpackedSample sample)
        {
            if (_list.Count == 0)
            {
                sample = default;
                return false;
            }

            sample = _list.First();
            return true;
        }

        public void Add(UnpackedSample sample)
        {
            var idx = _comparer == null ? _list.BinarySearch(sample) : _list.BinarySearch(sample, _comparer);
            _list.Insert(idx >= 0 ? idx : ~idx, sample);
        }

        public bool TryTake(out UnpackedSample sample)
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