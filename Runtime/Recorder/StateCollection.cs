using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PLUME.Recorder.Module;

namespace PLUME.Recorder
{
    public class StateCollection<T> : ICollection<T>, IReadOnlyCollection<T>, IStateCollection where T : IUnityObjectState
    {
        private T[] _samples;
        private int Capacity => _samples.Length;

        public int Count { get; private set; }

        int ICollection<T>.Count => Count;

        public bool IsSynchronized => false;

        public object SyncRoot { get; } = new();

        public bool IsReadOnly => false;

        public StateCollection()
        {
            _samples = Array.Empty<T>();
        }

        public StateCollection(int initialCapacity)
        {
            _samples = new T[initialCapacity];
        }

        public StateCollection(T[] initialSamples)
        {
            _samples = initialSamples;
            Count = initialSamples.Length;
        }

        public Memory<T> AsMemory()
        {
            return _samples.AsMemory(0, Count);
        }

        public Span<T> AsSpan()
        {
            return _samples.AsSpan(0, Count);
        }

        public void CopyTo(Array array, int index)
        {
            Array.Copy(_samples, 0, array, index, Count);
        }

        public Type GetSampleType()
        {
            return typeof(T);
        }

        public void Clear()
        {
            Count = 0;
        }

        void ICollection<T>.Clear()
        {
            Clear();
        }

        public IEnumerable<IUnityObjectState> AsSampleEnumerable()
        {
            return _samples.Cast<IUnityObjectState>();
        }

        public IEnumerable<T> AsTypedSampleEnumerable()
        {
            return _samples;
        }

        private void EnsureCapacity(int capacity)
        {
            if (capacity <= _samples.Length)
                return;
            Array.Resize(ref _samples, Math.Max(capacity, _samples.Length * 2));
        }

        public void Add(T item)
        {
            EnsureCapacity(Capacity + 1);
            _samples[Count++] = item;
        }
        
        public void AddRange(ReadOnlyMemory<T> samples)
        {
            AddRange(samples.Span);
        }
        
        public void AddRange(ReadOnlySpan<T> samples)
        {
            EnsureCapacity(Capacity + samples.Length);
            samples.CopyTo(_samples.AsSpan(Count, samples.Length));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _samples.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            for (var i = 0; i < Count; i++)
            {
                if (!_samples[i].Equals(item)) continue;
                Array.Copy(_samples, i + 1, _samples, i, Count - i - 1);
                Count--;
                return true;
            }

            return false;
        }

        public bool Contains(T item)
        {
            return _samples.Contains(item);
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(_samples, item, 0, Count);
        }

        public void Insert(int index, T item)
        {
            if (index < 0 || index > Count)
                throw new IndexOutOfRangeException();

            EnsureCapacity(Capacity + 1);
            Array.Copy(_samples, index, _samples, index + 1, Count - index);
            _samples[index] = item;
            Count++;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            Array.Copy(_samples, index + 1, _samples, index, Count - index - 1);
            Count--;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();
                return _samples[index];
            }
            set
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();
                _samples[index] = value;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_samples).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}