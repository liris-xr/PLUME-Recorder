using System.Collections.Generic;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Profiling;

namespace PLUME
{
    [DisallowMultipleComponent]
    public class TransformRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, Transform> _recordedTransforms = new();
        private readonly Dictionary<int, TransformGameObjectIdentifier> _cachedIdentifiers = new();
        
        private readonly Dictionary<int, int> _lastSiblingIndex = new();
        private readonly Dictionary<int, int?> _lastParentTransformId = new();
        private readonly Dictionary<int, Vector3> _lastScale = new();
        private readonly Dictionary<int, Vector3> _lastPosition = new();
        private readonly Dictionary<int, Quaternion> _lastRotation = new();

        protected override void ResetCache()
        {
            _recordedTransforms.Clear();
            _cachedIdentifiers.Clear();
            _lastSiblingIndex.Clear();
            _lastParentTransformId.Clear();
            _lastScale.Clear();
            _lastPosition.Clear();
            _lastRotation.Clear();
        }
        
        public void FixedUpdate()
        {
            if (_recordedTransforms.Count == 0)
                return;

            RecordUpdate();
        }

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is Transform t and not RectTransform)
            {
                if (!_recordedTransforms.ContainsKey(t.GetInstanceID()))
                {
                    var transformId = t.GetInstanceID();
                    _recordedTransforms.Add(transformId, t);
                    _cachedIdentifiers.Add(transformId, t.ToIdentifierPayload());
                    _lastSiblingIndex.Add(transformId, t.GetSiblingIndex());
                    _lastParentTransformId.Add(transformId, t.parent == null ? null : t.parent.GetInstanceID());
                    _lastScale.Add(transformId, t.transform.lossyScale);
                    _lastPosition.Add(transformId, t.transform.position);
                    _lastRotation.Add(transformId, t.transform.rotation);
                    RecordCreation(t);
                }
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedTransforms.ContainsKey(objectInstanceId))
            {
                var t = _recordedTransforms[objectInstanceId];

                if (t != null)
                {
                    RecordDestruction(objectInstanceId);
                    RemoveFromCache(objectInstanceId);
                }
            }
        }

        private void RemoveFromCache(int transformInstanceId)
        {
            _recordedTransforms.Remove(transformInstanceId);
            _lastSiblingIndex.Remove(transformInstanceId);
            _lastParentTransformId.Remove(transformInstanceId);
            _lastScale.Remove(transformInstanceId);
            _lastPosition.Remove(transformInstanceId);
            _lastRotation.Remove(transformInstanceId);
            _cachedIdentifiers.Remove(transformInstanceId);
        }

        private void RecordCreation(Transform t)
        {
            var transformId = t.GetInstanceID();
            var transformCreate = new TransformCreate {Id = _cachedIdentifiers[transformId]};
            var transformUpdateParent = CreateTransformUpdateParent(t);
            var transformUpdateSiblingIndex = CreateTransformUpdateSiblingIndex(t);

            t.GetPositionAndRotation(out var position, out var rotation);
            var lossyScale = t.lossyScale;
            t.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
            var localScale = t.localScale;

            var transformUpdatePosition = new TransformUpdatePosition
            {
                Id = _cachedIdentifiers[transformId],
                LocalPosition = new Sample.Common.Vector3
                    {X = localPosition.x, Y = localPosition.y, Z = localPosition.z},
                WorldPosition = new Sample.Common.Vector3 {X = position.x, Y = position.y, Z = position.z},
            };

            var transformUpdateRotation = new TransformUpdateRotation
            {
                Id = _cachedIdentifiers[transformId],
                LocalRotation = new Sample.Common.Quaternion
                    {X = localRotation.x, Y = localRotation.y, Z = localRotation.z, W = localRotation.w},
                WorldRotation = new Sample.Common.Quaternion
                    {X = rotation.x, Y = rotation.y, Z = rotation.z, W = rotation.w},
            };

            var transformUpdateScale = new TransformUpdateScale
            {
                Id = _cachedIdentifiers[transformId],
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

        private void RecordDestruction(int transformId)
        {
            var transformDestroy = new TransformDestroy {Id = _cachedIdentifiers[transformId]};
            recorder.RecordSample(transformDestroy);
        }

        // TODO add object pool for transform messages
        private void RecordUpdate()
        {
            Profiler.BeginSample("Recording transform");

            // Keep track of any transforms destroyed (ref == null) that were not picked up by the system.
            // This can happen if the object is not destroyed by calling Destroy or DestroyImmediate (in Editor or internal C++ engine)
            var nullTransformInstanceIds = new List<int>();

            foreach (var (transformId, t) in _recordedTransforms)
            {
                if (t == null)
                {
                    nullTransformInstanceIds.Add(transformId);
                    continue;
                }

                if (t.hasChanged)
                {
                    var parent = t.parent;

                    var lastParentTransformId = _lastParentTransformId[transformId];
                    var lastScale = _lastScale[transformId];
                    var lastPosition = _lastPosition[transformId];
                    var lastRotation = _lastRotation[transformId];

                    var parentHasChanged = parent == null && lastParentTransformId.HasValue ||
                                           parent != null && !lastParentTransformId.HasValue ||
                                           parent != null && lastParentTransformId.HasValue &&
                                           lastParentTransformId.Value != parent.GetHashCode();

                    if (parentHasChanged)
                    {
                        _lastParentTransformId[transformId] = parent == null ? null : t.parent.GetInstanceID();
                        var transformUpdateParent = CreateTransformUpdateParent(t);
                        _lastParentTransformId[transformId] = t.parent == null ? null : t.parent.GetInstanceID();
                        recorder.RecordSample(transformUpdateParent);
                    }

                    t.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
                    var localScale = t.localScale;
                    t.GetPositionAndRotation(out var position, out var rotation);
                    var scale = t.lossyScale;

                    if (lastPosition != position && lastRotation != rotation)
                    {
                        var transformUpdatePosition = new TransformUpdatePosition
                        {
                            Id = _cachedIdentifiers[transformId],
                            LocalPosition = localPosition.ToPayload(),
                            WorldPosition = position.ToPayload()
                        };
                        
                        var transformUpdateRotation = new TransformUpdateRotation
                        {
                            Id = _cachedIdentifiers[transformId],
                            LocalRotation = localRotation.ToPayload(),
                            WorldRotation = rotation.ToPayload()
                        };

                        recorder.RecordSample(transformUpdatePosition);
                        recorder.RecordSample(transformUpdateRotation);
                        _lastPosition[transformId] = position;
                        _lastRotation[transformId] = rotation;
                    }
                    else if (lastPosition != position)
                    {
                        var transformUpdatePosition = new TransformUpdatePosition
                        {
                            Id = _cachedIdentifiers[transformId],
                            LocalPosition = localPosition.ToPayload(),
                            WorldPosition = position.ToPayload()
                        };

                        recorder.RecordSample(transformUpdatePosition);
                        _lastPosition[transformId] = position;
                    }
                    else if (lastRotation != rotation)
                    {
                        var transformUpdateRotation = new TransformUpdateRotation
                        {
                            Id = _cachedIdentifiers[transformId],
                            LocalRotation = localRotation.ToPayload(),
                            WorldRotation = rotation.ToPayload()
                        };

                        recorder.RecordSample(transformUpdateRotation);
                        _lastRotation[transformId] = rotation;
                    }

                    if (lastScale != scale)
                    {
                        var transformUpdateScale = new TransformUpdateScale
                        {
                            Id = _cachedIdentifiers[transformId],
                            LocalScale = new Sample.Common.Vector3
                                {X = localScale.x, Y = localScale.y, Z = localScale.z},
                            WorldScale = new Sample.Common.Vector3
                                {X = scale.x, Y = scale.y, Z = scale.z}
                        };
                        recorder.RecordSample(transformUpdateScale);
                        _lastScale[transformId] = scale;
                    }

                    t.hasChanged = false;
                }

                var hasSiblingIndexChanged = _lastSiblingIndex[transformId] != t.GetSiblingIndex();

                if (hasSiblingIndexChanged)
                {
                    _lastSiblingIndex[transformId] = t.GetSiblingIndex();
                    var transformUpdateSiblingIndex = CreateTransformUpdateSiblingIndex(t);
                    recorder.RecordSample(transformUpdateSiblingIndex);
                }
            }

            foreach (var nullTransformInstanceId in nullTransformInstanceIds)
            {
                RecordDestruction(nullTransformInstanceId);
                RemoveFromCache(nullTransformInstanceId);
            }

            Profiler.EndSample();
        }

        private TransformUpdateSiblingIndex CreateTransformUpdateSiblingIndex(Transform t)
        {
            var transformUpdateSiblingIndex = new TransformUpdateSiblingIndex
            {
                Id = _cachedIdentifiers[t.GetInstanceID()],
                SiblingIndex = t.GetSiblingIndex()
            };

            return transformUpdateSiblingIndex;
        }

        private TransformUpdateParent CreateTransformUpdateParent(Transform t)
        {
            TransformGameObjectIdentifier parentIdentifier = null;

            var parent = t.parent;

            if (parent != null)
            {
                var parentId = parent.GetInstanceID();
                
                parentIdentifier = _cachedIdentifiers.ContainsKey(parentId)
                    ? _cachedIdentifiers[parentId]
                    : parent.ToIdentifierPayload();
            }

            var transformUpdateParent = new TransformUpdateParent
            {
                Id = _cachedIdentifiers[t.GetInstanceID()],
                ParentId = parentIdentifier
            };

            return transformUpdateParent;
        }
    }
}