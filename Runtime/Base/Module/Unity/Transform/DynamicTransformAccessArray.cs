using System;
using System.Collections.Generic;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using Unity.Collections;
using UnityEngine.Jobs;

namespace PLUME.Base.Module.Unity.Transform
{
    [GenerateTestsForBurstCompatibility]
    public class DynamicTransformAccessArray : IDisposable
    {
        private TransformAccessArray _transformAccessArray;
        private NativeList<ComponentIdentifier> _alignedIdentifiers;
        private readonly Dictionary<int, int> _instanceIdToIndex;

        public int Length => _transformAccessArray.length;
        public bool IsCreated => _transformAccessArray.isCreated;

        public DynamicTransformAccessArray(int initialCapacity = 1000, int desiredJobCount = -1)
        {
            _transformAccessArray = new TransformAccessArray(initialCapacity, desiredJobCount);
            _alignedIdentifiers = new NativeList<ComponentIdentifier>(initialCapacity, Allocator.Persistent);
            _instanceIdToIndex = new Dictionary<int, int>(initialCapacity);
        }

        public void Clear()
        {
            _transformAccessArray.SetTransforms(Array.Empty<UnityEngine.Transform>());
            _alignedIdentifiers.Clear();
            _instanceIdToIndex.Clear();
        }

        public bool TryAdd(ComponentSafeRef<UnityEngine.Transform> objRef)
        {
            if (Contains(objRef))
                return false;

            Add(objRef);
            return true;
        }

        public void Add(ComponentSafeRef<UnityEngine.Transform> objRef)
        {
            EnsureCapacity(_transformAccessArray.length + 1);

            if (Contains(objRef))
                throw new InvalidOperationException($"Transform {objRef.Component.name} is already in the list");

            _transformAccessArray.Add(objRef.Component);
            _alignedIdentifiers.Add(objRef.ComponentIdentifier);
            _instanceIdToIndex.Add(objRef.ComponentIdentifier.ComponentId.InstanceId, _transformAccessArray.length - 1);
        }

        private void EnsureCapacity(int capacity)
        {
            if (capacity <= _transformAccessArray.capacity)
                return;

            // If capacity is reached, double the capacity
            var newCapacity = Math.Max(capacity, _transformAccessArray.capacity * 2);
            var newTransformAccessArray = new TransformAccessArray(newCapacity);

            for (var i = 0; i < _transformAccessArray.length; i++)
            {
                newTransformAccessArray.Add(_transformAccessArray[i]);
            }

            _transformAccessArray.Dispose();
            _transformAccessArray = newTransformAccessArray;
        }

        /// <summary>
        /// Returns the index of the given transform in the list, or -1 if it is not in the list.
        /// </summary>
        /// <param name="objRef">The transform to find.</param>
        /// <returns>The index of the transform in the list, or -1 if it is not in the list.</returns>
        public int IndexOf(ComponentSafeRef<UnityEngine.Transform> objRef)
        {
            return _instanceIdToIndex.GetValueOrDefault(objRef.ComponentIdentifier.ComponentId.InstanceId, -1);
        }

        public bool Contains(ComponentSafeRef<UnityEngine.Transform> objRef)
        {
            return _instanceIdToIndex.ContainsKey(objRef.ComponentIdentifier.ComponentId.InstanceId);
        }

        /// <summary>
        /// Try to removes the given transform from the list.
        /// Might change the order of elements in the buffer as it swaps the last element with the one to remove.
        /// </summary>
        /// <param name="objRef">The transform to remove.</param>
        /// <returns>True if the transform was removed, false otherwise.</returns>
        public bool TryRemove(ComponentSafeRef<UnityEngine.Transform> objRef)
        {
            var index = IndexOf(objRef);

            if (index < 0)
                return false;

            RemoveAtSwapBack(index);
            return true;
        }

        /// <summary>
        /// Removes the given transform from the list and return. Throws an exception if the transform is not in the list.
        /// Might change the order of elements in the buffer as it swaps the last element with the one to remove.
        /// </summary>
        /// <param name="objRef">The transform to remove.</param>
        /// <exception cref="InvalidOperationException">Thrown if the transform is not in the list.</exception>
        /// <returns>The index of the removed transform.</returns>
        public int RemoveSwapBack(ComponentSafeRef<UnityEngine.Transform> objRef)
        {
            var index = IndexOf(objRef);

            if (index == -1)
                throw new InvalidOperationException($"Transform {objRef.Component.name} is not in the list");

            RemoveAtSwapBack(index);
            return index;
        }

        /// <summary>
        /// Removes the transform at the given index from the list. Throws an exception if the index is out of range.
        /// Might change the order of elements in the buffer as it swaps the last element with the one to remove.
        /// </summary>
        /// <param name="index">The index of the transform to remove.</param>
        /// <exception cref="InvalidOperationException">Thrown if the index is out of range.</exception>
        public void RemoveAtSwapBack(int index)
        {
            if (Length == 0)
                throw new InvalidOperationException("Cannot remove from an empty array");
            if (index >= Length || index < 0)
                throw new IndexOutOfRangeException($"Index {index} is out of range. Length is {Length}");

            var lastIdentifier = _alignedIdentifiers[Length - 1];
            var identifier = _alignedIdentifiers[index];

            _transformAccessArray.RemoveAtSwapBack(index);
            _alignedIdentifiers.RemoveAtSwapBack(index);
            _instanceIdToIndex.Remove(identifier.ComponentId.InstanceId);

            // Update the index of swapped element, in the case where the removed element was the last one
            // then instanceId == lastInstanceId and we do not need to update the index.
            if (identifier.ComponentId.InstanceId != lastIdentifier.ComponentId.InstanceId)
            {
                _instanceIdToIndex[lastIdentifier.ComponentId.InstanceId] = index;
            }
        }

        public UnityEngine.Transform this[int index] => _transformAccessArray[index];

        public static implicit operator TransformAccessArray(DynamicTransformAccessArray accessArray)
        {
            return accessArray._transformAccessArray;
        }

        public TransformAccessArray GetTransformAccessArray()
        {
            return _transformAccessArray;
        }

        public NativeList<ComponentIdentifier> GetAlignedIdentifiers()
        {
            return _alignedIdentifiers;
        }

        public void Dispose()
        {
            _transformAccessArray.Dispose();
            _alignedIdentifiers.Dispose();
        }
    }
}