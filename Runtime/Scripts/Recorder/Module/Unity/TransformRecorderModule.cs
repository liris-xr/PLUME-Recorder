using System.Collections.Generic;
using PLUME.Sample.Unity;
using Runtime;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Profiling;

namespace PLUME
{
    [DisallowMultipleComponent]
    public class TransformRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly List<ObjectNullSafeReference<Transform>> _recordedTransformsRefs = new();

        private readonly Dictionary<ObjectNullSafeReference<Transform>, TransformGameObjectIdentifier>
            _cachedIdentifiers = new();

        private readonly Dictionary<ObjectNullSafeReference<Transform>, int> _lastSiblingIndex = new();
        private readonly Dictionary<ObjectNullSafeReference<Transform>, int?> _lastParentTransformId = new();
        private readonly Dictionary<ObjectNullSafeReference<Transform>, Vector3> _lastLocalScale = new();
        private readonly Dictionary<ObjectNullSafeReference<Transform>, Vector3> _lastLocalPosition = new();
        private readonly Dictionary<ObjectNullSafeReference<Transform>, Quaternion> _lastLocalRotation = new();

        public void FixedUpdate()
        {
            if (_recordedTransformsRefs.Count == 0)
                return;

            RecordUpdate();
        }

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is Transform t and not RectTransform)
            {
                var transformRef = new ObjectNullSafeReference<Transform>(t);

                if (!_recordedTransformsRefs.Contains(transformRef))
                {
                    _recordedTransformsRefs.Add(transformRef);
                    _cachedIdentifiers.Add(transformRef, t.ToIdentifierPayload());
                    _lastSiblingIndex.Add(transformRef, t.GetSiblingIndex());
                    _lastParentTransformId.Add(transformRef, t.parent == null ? null : t.parent.GetInstanceID());
                    _lastLocalScale.Add(transformRef, t.transform.localScale);
                    _lastLocalPosition.Add(transformRef, t.transform.localPosition);
                    _lastLocalRotation.Add(transformRef, t.transform.localRotation);
                    RecordCreation(transformRef);
                }
            }
        }

        public void OnStopRecordingObject(Object obj)
        {
            if (obj is Transform t)
            {
                var transformRef = new ObjectNullSafeReference<Transform>(t);

                if (_recordedTransformsRefs.Contains(transformRef))
                {
                    RecordDestruction(transformRef);
                    _recordedTransformsRefs.Remove(transformRef);
                    _lastSiblingIndex.Remove(transformRef);
                    _lastParentTransformId.Remove(transformRef);
                    _lastLocalScale.Remove(transformRef);
                    _lastLocalPosition.Remove(transformRef);
                    _lastLocalRotation.Remove(transformRef);
                    _cachedIdentifiers.Remove(transformRef);
                }
            }
        }

        private void RecordCreation(ObjectNullSafeReference<Transform> transformRef)
        {
            var transformCreate = new TransformCreate {Id = _cachedIdentifiers[transformRef]};
            var transformUpdateParent = CreateTransformUpdateParent(transformRef);
            var transformUpdateSiblingIndex = CreateTransformUpdateSiblingIndex(transformRef);

            var t = transformRef.Object;
            t.GetPositionAndRotation(out var position, out var rotation);
            var lossyScale = t.lossyScale;
            t.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
            var localScale = t.localScale;

            var transformUpdatePosition = new TransformUpdatePosition
            {
                Id = _cachedIdentifiers[transformRef],
                LocalPosition = new Sample.Common.Vector3
                    {X = localPosition.x, Y = localPosition.y, Z = localPosition.z},
                WorldPosition = new Sample.Common.Vector3 {X = position.x, Y = position.y, Z = position.z},
            };

            var transformUpdateRotation = new TransformUpdateRotation
            {
                Id = _cachedIdentifiers[transformRef],
                LocalRotation = new Sample.Common.Quaternion
                    {X = localRotation.x, Y = localRotation.y, Z = localRotation.z, W = localRotation.w},
                WorldRotation = new Sample.Common.Quaternion
                    {X = rotation.x, Y = rotation.y, Z = rotation.z, W = rotation.w},
            };

            var transformUpdateScale = new TransformUpdateScale
            {
                Id = _cachedIdentifiers[transformRef],
                LocalScale = new Sample.Common.Vector3 {X = localScale.x, Y = localScale.y, Z = localScale.z},
                WorldScale = new Sample.Common.Vector3 {X = lossyScale.x, Y = lossyScale.y, Z = lossyScale.z}
            };

            recorder.RecordSample(transformCreate);
            recorder.RecordSample(transformUpdateParent);
            recorder.RecordSample(transformUpdateSiblingIndex);
            recorder.RecordSample(transformUpdatePosition);
            recorder.RecordSample(transformUpdateRotation);
            recorder.RecordSample(transformUpdateScale);
        }

        private void RecordDestruction(ObjectNullSafeReference<Transform> transformRef)
        {
            var transformDestroy = new TransformDestroy {Id = _cachedIdentifiers[transformRef]};
            recorder.RecordSample(transformDestroy);
        }

        // TODO add object pool for transform messages
        private void RecordUpdate()
        {
            Profiler.BeginSample("Recording transform");

            // Keep track of any transforms destroyed (ref == null) that were not picked up by the system.
            // This can happen if the object is not destroyed by calling Destroy or DestroyImmediate (in Editor or internal C++ engine)
            var nullTransformRefs = new List<ObjectNullSafeReference<Transform>>();

            foreach (var transformRef in _recordedTransformsRefs)
            {
                // Prevent MissingReferenceException and record object destruction in a clean way
                if (transformRef.HasBeenDestroyed())
                {
                    nullTransformRefs.Add(transformRef);
                    RecordDestruction(transformRef);
                    continue;
                }

                var t = transformRef.Object;

                if (t.hasChanged)
                {
                    var parent = t.parent;

                    var lastParentTransformId = _lastParentTransformId[transformRef];
                    var lastLocalScale = _lastLocalScale[transformRef];
                    var lastLocalPosition = _lastLocalPosition[transformRef];
                    var lastLocalRotation = _lastLocalRotation[transformRef];

                    var parentHasChanged = parent == null && lastParentTransformId.HasValue ||
                                           parent != null && !lastParentTransformId.HasValue ||
                                           parent != null && lastParentTransformId.HasValue &&
                                           lastParentTransformId.Value != parent.GetHashCode();

                    if (parentHasChanged)
                    {
                        _lastParentTransformId[transformRef] = parent == null ? null : t.parent.GetInstanceID();
                        var transformUpdateParent = CreateTransformUpdateParent(transformRef);
                        _lastParentTransformId[transformRef] = t.parent == null ? null : t.parent.GetInstanceID();
                        recorder.RecordSample(transformUpdateParent);
                    }

                    t.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
                    var localScale = t.localScale;

                    if (lastLocalPosition != localPosition && lastLocalRotation != localRotation)
                    {
                        // Use the faster alternative to get position and rotation at the same time
                        t.GetPositionAndRotation(out var position, out var rotation);

                        var transformUpdatePosition = new TransformUpdatePosition
                        {
                            Id = _cachedIdentifiers[transformRef],
                            LocalPosition = localPosition.ToPayload(),
                            WorldPosition = position.ToPayload()
                        };
                        
                        var transformUpdateRotation = new TransformUpdateRotation
                        {
                            Id = _cachedIdentifiers[transformRef],
                            LocalRotation = localRotation.ToPayload(),
                            WorldRotation = rotation.ToPayload()
                        };

                        recorder.RecordSample(transformUpdatePosition);
                        recorder.RecordSample(transformUpdateRotation);
                        _lastLocalPosition[transformRef] = localPosition;
                        _lastLocalRotation[transformRef] = localRotation;
                    }
                    else if (lastLocalPosition != localPosition)
                    {
                        var transformUpdatePosition = new TransformUpdatePosition
                        {
                            Id = _cachedIdentifiers[transformRef],
                            LocalPosition = localPosition.ToPayload(),
                            WorldPosition = t.transform.position.ToPayload()
                        };

                        recorder.RecordSample(transformUpdatePosition);
                        _lastLocalPosition[transformRef] = localPosition;
                    }
                    else if (lastLocalRotation != localRotation)
                    {
                        var transformUpdateRotation = new TransformUpdateRotation
                        {
                            Id = _cachedIdentifiers[transformRef],
                            LocalRotation = localRotation.ToPayload(),
                            WorldRotation = t.transform.rotation.ToPayload()
                        };

                        recorder.RecordSample(transformUpdateRotation);
                        _lastLocalRotation[transformRef] = localRotation;
                    }

                    if (lastLocalScale != localScale)
                    {
                        var lossyScale = t.lossyScale;
                        
                        var transformUpdateScale = new TransformUpdateScale
                        {
                            Id = _cachedIdentifiers[transformRef],
                            LocalScale = new Sample.Common.Vector3
                                {X = localScale.x, Y = localScale.y, Z = localScale.z},
                            WorldScale = new Sample.Common.Vector3
                                {X = lossyScale.x, Y = lossyScale.y, Z = lossyScale.z}
                        };
                        recorder.RecordSample(transformUpdateScale);
                        _lastLocalScale[transformRef] = localScale;
                    }

                    t.hasChanged = false;
                }

                var hasSiblingIndexChanged = _lastSiblingIndex[transformRef] != t.GetSiblingIndex();

                if (hasSiblingIndexChanged)
                {
                    _lastSiblingIndex[transformRef] = t.GetSiblingIndex();
                    var transformUpdateSiblingIndex = CreateTransformUpdateSiblingIndex(transformRef);
                    recorder.RecordSample(transformUpdateSiblingIndex);
                }
            }

            foreach (var nullTransformRef in nullTransformRefs)
            {
                _recordedTransformsRefs.Remove(nullTransformRef);
            }

            Profiler.EndSample();
        }

        private TransformUpdateSiblingIndex CreateTransformUpdateSiblingIndex(
            ObjectNullSafeReference<Transform> transformRef)
        {
            var t = transformRef.Object;

            var transformUpdateSiblingIndex = new TransformUpdateSiblingIndex
            {
                Id = _cachedIdentifiers[transformRef],
                SiblingIndex = t.GetSiblingIndex()
            };

            return transformUpdateSiblingIndex;
        }

        private TransformUpdateParent CreateTransformUpdateParent(ObjectNullSafeReference<Transform> transformRef)
        {
            TransformGameObjectIdentifier parentIdentifier = null;

            var t = transformRef.Object;
            var parent = t.parent;

            if (parent != null)
            {
                var parentRef = new ObjectNullSafeReference<Transform>(parent);

                parentIdentifier = _cachedIdentifiers.ContainsKey(parentRef)
                    ? _cachedIdentifiers[parentRef]
                    : parent.ToIdentifierPayload();
            }

            var transformUpdateParent = new TransformUpdateParent
            {
                Id = _cachedIdentifiers[transformRef],
                ParentId = parentIdentifier
            };

            return transformUpdateParent;
        }
    }
}