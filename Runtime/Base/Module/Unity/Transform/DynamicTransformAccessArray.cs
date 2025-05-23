using System;
using System.Collections.Generic;
using PLUME.Core;
using PLUME.Core.Object.SafeRef;
using PLUME.Sample.ProtoBurst.Unity;
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

        public bool TryAdd(IComponentSafeRef<UnityEngine.Transform> objRef)
        {
            if (Contains(objRef))
                return false;

            Add(objRef);
            return true;
        }

        public void Add(IComponentSafeRef<UnityEngine.Transform> objRef)
        {
            EnsureCapacity(_transformAccessArray.length + 1);

            if (Contains(objRef))
                throw new InvalidOperationException($"Transform {objRef.Component.name} is already in the list");

            _transformAccessArray.Add(objRef.Component);
            _alignedIdentifiers.Add(objRef.Identifier);
            _instanceIdToIndex.Add(objRef.Identifier.RuntimeId, _transformAccessArray.length - 1);
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
        public int IndexOf(IComponentSafeRef<UnityEngine.Transform> objRef)
        {
            return _instanceIdToIndex.GetValueOrDefault(objRef.Identifier.RuntimeId, -1);
        }

        public bool Contains(IComponentSafeRef<UnityEngine.Transform> objRef)
        {
            return _instanceIdToIndex.ContainsKey(objRef.Identifier.RuntimeId);
        }

        /// <summary>
        /// Try to removes the given transform from the list.
        /// Might change the order of elements in the buffer as it swaps the last element with the one to remove.
        /// </summary>
        /// <param name="objRef">The transform to remove.</param>
        /// <returns>True if the transform was removed, false otherwise.</returns>
        public bool TryRemove(IComponentSafeRef<UnityEngine.Transform> objRef)
        {
            var index = IndexOf(objRef);

            if (index < 0)
                return false;

            RemoveAtSwapBack(index);
            return true;
        }

        /// <summary>
        /// Removes the given transform from the list and return.
        /// Might change the order of elements in the buffer as it swaps the last element with the one to remove.
        /// </summary>
        /// <param name="objRef">The transform to remove.</param>
        /// <param name="index">The index where the transform was removed, or -1 if no object was removed.</param>
        /// <returns>True if the transform was removed, false otherwise.</returns>
        public bool TryRemoveSwapBack(IComponentSafeRef<UnityEngine.Transform> objRef, out int index)
        {
            index = IndexOf(objRef);

            // If the object is not in the list, we do not need to do anything
            if (index == -1)
                return false;

            RemoveAtSwapBack(index);
            return true;
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
            _instanceIdToIndex.Remove(identifier.RuntimeId);

            // Update the index of swapped element, in the case where the removed element was the last one
            // then instanceId == lastInstanceId and we do not need to update the index.
            if (identifier.RuntimeId != lastIdentifier.RuntimeId)
            {
                _instanceIdToIndex[lastIdentifier.RuntimeId] = index;
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
            if (_transformAccessArray.isCreated)
            {
                _transformAccessArray.Dispose();
                _transformAccessArray = default;
            }

            _alignedIdentifiers.Dispose();
            _alignedIdentifiers = default;
        }
    }
}