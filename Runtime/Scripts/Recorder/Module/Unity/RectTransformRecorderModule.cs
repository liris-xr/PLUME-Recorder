using System.Collections.Generic;
using PLUME.Sample.Unity;
using Runtime;
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
        private readonly List<ObjectNullSafeReference<Transform>> _recordedTransformsRefs = new();

        private readonly Dictionary<ObjectNullSafeReference<Transform>, TransformGameObjectIdentifier>
            _cachedIdentifiers = new();

        private readonly Dictionary<ObjectNullSafeReference<Transform>, int> _lastSiblingIndex = new();
        private readonly Dictionary<ObjectNullSafeReference<Transform>, int?> _lastParentTransformId = new();

        public void FixedUpdate()
        {
            if (_recordedTransformsRefs.Count == 0)
                return;

            RecordUpdate();
        }

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is RectTransform rt)
            {
                var transformRef = new ObjectNullSafeReference<Transform>(rt);

                if (!_recordedTransformsRefs.Contains(transformRef))
                {
                    _recordedTransformsRefs.Add(transformRef);
                    _cachedIdentifiers.Add(transformRef, rt.ToIdentifierPayload());
                    _lastSiblingIndex.Add(transformRef, rt.GetSiblingIndex());
                    _lastParentTransformId.Add(transformRef, rt.parent == null ? null : rt.parent.GetInstanceID());
                    RecordCreation(transformRef);
                }
            }
        }

        public void OnStopRecordingObject(Object obj)
        {
            if (obj is RectTransform t)
            {
                var transformRef = new ObjectNullSafeReference<Transform>(t);

                if (_recordedTransformsRefs.Contains(transformRef))
                {
                    RecordDestruction(transformRef);
                    _recordedTransformsRefs.Remove(transformRef);
                    _lastSiblingIndex.Remove(transformRef);
                    _lastParentTransformId.Remove(transformRef);
                    _cachedIdentifiers.Remove(transformRef);
                }
            }
        }

        private void RecordCreation(ObjectNullSafeReference<Transform> transformRef)
        {
            var rectTransformCreate = new RectTransformCreate {Id = _cachedIdentifiers[transformRef]};
            var rectTransformUpdateParent = CreateTransformUpdateParent(transformRef);
            var transformUpdateSiblingIndex = CreateTransformUpdateSiblingIndex(transformRef);

            var rt = transformRef.Object as RectTransform;

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
                Id = _cachedIdentifiers[transformRef],
                LocalPosition = new Vector3 {X = localPosition.x, Y = localPosition.y, Z = localPosition.z},
                WorldPosition = new Vector3 {X = position.x, Y = position.y, Z = position.z}
            };

            var rectTransformUpdateRotation = new RectTransformUpdateRotation
            {
                Id = _cachedIdentifiers[transformRef],
                LocalRotation = new Quaternion
                    {X = localRotation.x, Y = localRotation.y, Z = localRotation.z, W = localRotation.w},
                WorldRotation = new Quaternion {X = rotation.x, Y = rotation.y, Z = rotation.z, W = rotation.w}
            };

            var rectTransformUpdateScale = new RectTransformUpdateScale
            {
                Id = _cachedIdentifiers[transformRef],
                LocalScale = new Vector3 {X = localScale.x, Y = localScale.y, Z = localScale.z},
                WorldScale = new Vector3 {X = lossyScale.x, Y = lossyScale.y, Z = lossyScale.z}
            };

            var rectTransformUpdateAnchors = new RectTransformUpdateAnchors
            {
                Id = _cachedIdentifiers[transformRef],
                AnchorMin = new Vector2 {X = anchorMin.x, Y = anchorMin.y},
                AnchorMax = new Vector2 {X = anchorMax.x, Y = anchorMax.y}
            };

            var rectTransformUpdateSize = new RectTransformUpdateSize
            {
                Id = _cachedIdentifiers[transformRef],
                SizeDelta = new Vector2 {X = sizeDelta.x, Y = sizeDelta.y}
            };

            var rectTransformUpdatePivot = new RectTransformUpdatePivot
            {
                Id = _cachedIdentifiers[transformRef],
                Pivot = new Vector2 {X = pivot.x, Y = pivot.y}
            };

            recorder.RecordSample(rectTransformUpdatePosition);
            recorder.RecordSample(rectTransformUpdateRotation);
            recorder.RecordSample(rectTransformUpdateScale);
            recorder.RecordSample(rectTransformUpdateAnchors);
            recorder.RecordSample(rectTransformUpdateSize);
            recorder.RecordSample(rectTransformUpdatePivot);
        }

        private void RecordDestruction(ObjectNullSafeReference<Transform> transformRef)
        {
            var rectTransformDestroy = new RectTransformDestroy {Id = _cachedIdentifiers[transformRef]};
            recorder.RecordSample(rectTransformDestroy);
        }

        private void RecordUpdate()
        {
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


                    var rt = transformRef.Object as RectTransform;

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
                        Id = _cachedIdentifiers[transformRef],
                        LocalPosition = new Vector3 {X = localPosition.x, Y = localPosition.y, Z = localPosition.z},
                        WorldPosition = new Vector3 {X = position.x, Y = position.y, Z = position.z}
                    };

                    var rectTransformUpdateRotation = new RectTransformUpdateRotation
                    {
                        Id = _cachedIdentifiers[transformRef],
                        LocalRotation = new Quaternion
                            {X = localRotation.x, Y = localRotation.y, Z = localRotation.z, W = localRotation.w},
                        WorldRotation = new Quaternion {X = rotation.x, Y = rotation.y, Z = rotation.z, W = rotation.w}
                    };

                    var rectTransformUpdateScale = new RectTransformUpdateScale
                    {
                        Id = _cachedIdentifiers[transformRef],
                        LocalScale = new Vector3 {X = localScale.x, Y = localScale.y, Z = localScale.z},
                        WorldScale = new Vector3 {X = lossyScale.x, Y = lossyScale.y, Z = lossyScale.z}
                    };

                    var rectTransformUpdateAnchors = new RectTransformUpdateAnchors
                    {
                        Id = _cachedIdentifiers[transformRef],
                        AnchorMin = new Vector2 {X = anchorMin.x, Y = anchorMin.y},
                        AnchorMax = new Vector2 {X = anchorMax.x, Y = anchorMax.y}
                    };

                    var rectTransformUpdateSize = new RectTransformUpdateSize
                    {
                        Id = _cachedIdentifiers[transformRef],
                        SizeDelta = new Vector2 {X = sizeDelta.x, Y = sizeDelta.y}
                    };

                    var rectTransformUpdatePivot = new RectTransformUpdatePivot
                    {
                        Id = _cachedIdentifiers[transformRef],
                        Pivot = new Vector2 {X = pivot.x, Y = pivot.y}
                    };

                    recorder.RecordSample(rectTransformUpdatePosition);
                    recorder.RecordSample(rectTransformUpdateRotation);
                    recorder.RecordSample(rectTransformUpdateScale);
                    recorder.RecordSample(rectTransformUpdateAnchors);
                    recorder.RecordSample(rectTransformUpdateSize);
                    recorder.RecordSample(rectTransformUpdatePivot);
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
        }

        private RectTransformUpdateSiblingIndex CreateTransformUpdateSiblingIndex(
            ObjectNullSafeReference<Transform> transformRef)
        {
            var t = transformRef.Object;

            var rectTransformUpdateSiblingIndex = new RectTransformUpdateSiblingIndex
            {
                Id = _cachedIdentifiers[transformRef],
                SiblingIndex = t.GetSiblingIndex()
            };

            return rectTransformUpdateSiblingIndex;
        }

        private RectTransformUpdateParent CreateTransformUpdateParent(ObjectNullSafeReference<Transform> transformRef)
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

            var rectTransformUpdateParent = new RectTransformUpdateParent
            {
                Id = _cachedIdentifiers[transformRef],
                ParentId = parentIdentifier
            };

            return rectTransformUpdateParent;
        }
    }
}