using System.Collections.Generic;
using PLUME.Sample.Unity;
using UnityEngine;
using Quaternion = PLUME.Sample.Common.Quaternion;
using Vector2 = PLUME.Sample.Common.Vector2;
using Vector3 = PLUME.Sample.Common.Vector3;

namespace PLUME
{
    [DisallowMultipleComponent]
    public class RectTransformRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, RectTransform> _recordedRectTransforms = new();
        private readonly Dictionary<int, TransformGameObjectIdentifier> _cachedIdentifiers = new();
        private readonly Dictionary<int, int> _lastSiblingIndex = new();
        private readonly Dictionary<int, int?> _lastParentTransformId = new();

        public void FixedUpdate()
        {
            if (_recordedRectTransforms.Count == 0)
                return;

            RecordUpdate();
        }

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is RectTransform rectTransform)
            {
                var rectTransformInstanceId = rectTransform.GetInstanceID();

                if (!_recordedRectTransforms.ContainsKey(rectTransformInstanceId))
                {
                    _recordedRectTransforms.Add(rectTransformInstanceId, rectTransform);
                    _cachedIdentifiers.Add(rectTransformInstanceId, rectTransform.ToIdentifierPayload());
                    _lastSiblingIndex.Add(rectTransformInstanceId, rectTransform.GetSiblingIndex());
                    _lastParentTransformId.Add(rectTransformInstanceId,
                        rectTransform.parent == null ? null : rectTransform.parent.GetInstanceID());
                    RecordCreation(rectTransform);
                }
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (_recordedRectTransforms.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
        }

        private void RecordCreation(RectTransform rt)
        {
            var rectTransformInstanceId = rt.GetInstanceID();
            var rectTransformCreate = new RectTransformCreate {Id = _cachedIdentifiers[rectTransformInstanceId]};
            var rectTransformUpdateParent = CreateTransformUpdateParent(rt);
            var transformUpdateSiblingIndex = CreateTransformUpdateSiblingIndex(rt);

            rt.GetPositionAndRotation(out var position, out var rotation);
            var lossyScale = rt.lossyScale;
            rt.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
            var localScale = rt.localScale;
            var sizeDelta = rt.sizeDelta;
            var anchorMin = rt.anchorMin;
            var anchorMax = rt.anchorMax;
            var pivot = rt.pivot;

            var rectTransformUpdatePosition = new RectTransformUpdatePosition
            {
                Id = _cachedIdentifiers[rectTransformInstanceId],
                LocalPosition = new Vector3 {X = localPosition.x, Y = localPosition.y, Z = localPosition.z},
                WorldPosition = new Vector3 {X = position.x, Y = position.y, Z = position.z}
            };

            var rectTransformUpdateRotation = new RectTransformUpdateRotation
            {
                Id = _cachedIdentifiers[rectTransformInstanceId],
                LocalRotation = new Quaternion
                    {X = localRotation.x, Y = localRotation.y, Z = localRotation.z, W = localRotation.w},
                WorldRotation = new Quaternion {X = rotation.x, Y = rotation.y, Z = rotation.z, W = rotation.w}
            };

            var rectTransformUpdateScale = new RectTransformUpdateScale
            {
                Id = _cachedIdentifiers[rectTransformInstanceId],
                LocalScale = new Vector3 {X = localScale.x, Y = localScale.y, Z = localScale.z},
                WorldScale = new Vector3 {X = lossyScale.x, Y = lossyScale.y, Z = lossyScale.z}
            };

            var rectTransformUpdateAnchors = new RectTransformUpdateAnchors
            {
                Id = _cachedIdentifiers[rectTransformInstanceId],
                AnchorMin = new Vector2 {X = anchorMin.x, Y = anchorMin.y},
                AnchorMax = new Vector2 {X = anchorMax.x, Y = anchorMax.y}
            };

            var rectTransformUpdateSize = new RectTransformUpdateSize
            {
                Id = _cachedIdentifiers[rectTransformInstanceId],
                SizeDelta = new Vector2 {X = sizeDelta.x, Y = sizeDelta.y}
            };

            var rectTransformUpdatePivot = new RectTransformUpdatePivot
            {
                Id = _cachedIdentifiers[rectTransformInstanceId],
                Pivot = new Vector2 {X = pivot.x, Y = pivot.y}
            };

            recorder.RecordSample(rectTransformUpdatePosition);
            recorder.RecordSample(rectTransformUpdateRotation);
            recorder.RecordSample(rectTransformUpdateScale);
            recorder.RecordSample(rectTransformUpdateAnchors);
            recorder.RecordSample(rectTransformUpdateSize);
            recorder.RecordSample(rectTransformUpdatePivot);
        }

        private void RecordDestruction(int rectTransformInstanceId)
        {
            var rectTransformDestroy = new RectTransformDestroy {Id = _cachedIdentifiers[rectTransformInstanceId]};
            recorder.RecordSample(rectTransformDestroy);
        }

        private void RecordUpdate()
        {
            // Keep track of any transforms destroyed (ref == null) that were not picked up by the system.
            // This can happen if the object is not destroyed by calling Destroy or DestroyImmediate (in Editor or internal C++ engine)
            var nullTransformInstanceIds = new List<int>();

            foreach (var (rectTransformInstanceId, rt) in _recordedRectTransforms)
            {
                if (rt == null)
                {
                    nullTransformInstanceIds.Add(rectTransformInstanceId);
                    continue;
                }

                if (rt.hasChanged)
                {
                    var parent = rt.parent;

                    var lastParentTransformId = _lastParentTransformId[rectTransformInstanceId];

                    var parentHasChanged = parent == null && lastParentTransformId.HasValue ||
                                           parent != null && !lastParentTransformId.HasValue ||
                                           parent != null && lastParentTransformId.HasValue &&
                                           lastParentTransformId.Value != parent.GetHashCode();

                    if (parentHasChanged)
                    {
                        _lastParentTransformId[rectTransformInstanceId] =
                            parent == null ? null : rt.parent.GetInstanceID();
                        var transformUpdateParent = CreateTransformUpdateParent(rt);
                        _lastParentTransformId[rectTransformInstanceId] =
                            rt.parent == null ? null : rt.parent.GetInstanceID();
                        recorder.RecordSample(transformUpdateParent);
                    }

                    rt.GetPositionAndRotation(out var position, out var rotation);
                    var lossyScale = rt.lossyScale;
                    rt.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
                    var localScale = rt.localScale;
                    var sizeDelta = rt.sizeDelta;
                    var anchorMin = rt.anchorMin;
                    var anchorMax = rt.anchorMax;
                    var pivot = rt.pivot;

                    var rectTransformUpdatePosition = new RectTransformUpdatePosition
                    {
                        Id = _cachedIdentifiers[rectTransformInstanceId],
                        LocalPosition = new Vector3 {X = localPosition.x, Y = localPosition.y, Z = localPosition.z},
                        WorldPosition = new Vector3 {X = position.x, Y = position.y, Z = position.z}
                    };

                    var rectTransformUpdateRotation = new RectTransformUpdateRotation
                    {
                        Id = _cachedIdentifiers[rectTransformInstanceId],
                        LocalRotation = new Quaternion
                            {X = localRotation.x, Y = localRotation.y, Z = localRotation.z, W = localRotation.w},
                        WorldRotation = new Quaternion {X = rotation.x, Y = rotation.y, Z = rotation.z, W = rotation.w}
                    };

                    var rectTransformUpdateScale = new RectTransformUpdateScale
                    {
                        Id = _cachedIdentifiers[rectTransformInstanceId],
                        LocalScale = new Vector3 {X = localScale.x, Y = localScale.y, Z = localScale.z},
                        WorldScale = new Vector3 {X = lossyScale.x, Y = lossyScale.y, Z = lossyScale.z}
                    };

                    var rectTransformUpdateAnchors = new RectTransformUpdateAnchors
                    {
                        Id = _cachedIdentifiers[rectTransformInstanceId],
                        AnchorMin = new Vector2 {X = anchorMin.x, Y = anchorMin.y},
                        AnchorMax = new Vector2 {X = anchorMax.x, Y = anchorMax.y}
                    };

                    var rectTransformUpdateSize = new RectTransformUpdateSize
                    {
                        Id = _cachedIdentifiers[rectTransformInstanceId],
                        SizeDelta = new Vector2 {X = sizeDelta.x, Y = sizeDelta.y}
                    };

                    var rectTransformUpdatePivot = new RectTransformUpdatePivot
                    {
                        Id = _cachedIdentifiers[rectTransformInstanceId],
                        Pivot = new Vector2 {X = pivot.x, Y = pivot.y}
                    };

                    recorder.RecordSample(rectTransformUpdatePosition);
                    recorder.RecordSample(rectTransformUpdateRotation);
                    recorder.RecordSample(rectTransformUpdateScale);
                    recorder.RecordSample(rectTransformUpdateAnchors);
                    recorder.RecordSample(rectTransformUpdateSize);
                    recorder.RecordSample(rectTransformUpdatePivot);
                    rt.hasChanged = false;
                }

                var hasSiblingIndexChanged = _lastSiblingIndex[rectTransformInstanceId] != rt.GetSiblingIndex();

                if (hasSiblingIndexChanged)
                {
                    _lastSiblingIndex[rectTransformInstanceId] = rt.GetSiblingIndex();
                    var transformUpdateSiblingIndex = CreateTransformUpdateSiblingIndex(rt);
                    recorder.RecordSample(transformUpdateSiblingIndex);
                }
            }

            foreach (var nullTransformInstanceId in nullTransformInstanceIds)
            {
                RecordDestruction(nullTransformInstanceId);
                RemoveFromCache(nullTransformInstanceId);
            }
        }

        private void RemoveFromCache(int transformInstanceId)
        {
            _recordedRectTransforms.Remove(transformInstanceId);
            _cachedIdentifiers.Remove(transformInstanceId);
            _lastParentTransformId.Remove(transformInstanceId);
            _lastSiblingIndex.Remove(transformInstanceId);
        }

        private RectTransformUpdateSiblingIndex CreateTransformUpdateSiblingIndex(RectTransform t)
        {
            var transformId = t.GetInstanceID();
            var rectTransformUpdateSiblingIndex = new RectTransformUpdateSiblingIndex
            {
                Id = _cachedIdentifiers[transformId],
                SiblingIndex = t.GetSiblingIndex()
            };

            return rectTransformUpdateSiblingIndex;
        }

        private RectTransformUpdateParent CreateTransformUpdateParent(Transform t)
        {
            TransformGameObjectIdentifier parentIdentifier = null;

            var parent = t.parent;

            if (parent != null)
            {
                var parentInstanceId = parent.GetInstanceID();

                parentIdentifier = _cachedIdentifiers.ContainsKey(parentInstanceId)
                    ? _cachedIdentifiers[parentInstanceId]
                    : parent.ToIdentifierPayload();
            }

            var rectTransformUpdateParent = new RectTransformUpdateParent
            {
                Id = _cachedIdentifiers[t.GetInstanceID()],
                ParentId = parentIdentifier
            };

            return rectTransformUpdateParent;
        }

        protected override void ResetCache()
        {
            _recordedRectTransforms.Clear();
            _cachedIdentifiers.Clear();
            _lastParentTransformId.Clear();
            _lastSiblingIndex.Clear();
        }
    }
}