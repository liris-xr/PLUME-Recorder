using System.Collections.Generic;
using System.Diagnostics;
using PLUME.Sample.Unity;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PLUME
{
    [DisallowMultipleComponent]
    public class TransformRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        public struct CachedTransformGameObjectIdentifier
        {
            public string TransformId;
            public string GameObjectId;

            public TransformGameObjectIdentifier ToPayload()
            {
                return new TransformGameObjectIdentifier
                {
                    TransformId = TransformId,
                    GameObjectId = GameObjectId
                };
            }
        }
        
        private readonly Dictionary<int, Transform> _recordedTransforms = new();
        private readonly Dictionary<int, CachedTransformGameObjectIdentifier> _cachedIdentifiers = new();

        private readonly Dictionary<int, int> _lastSiblingIndex = new();
        private readonly Dictionary<int, int?> _lastParentTransformId = new();
        private readonly Dictionary<int, Vector3> _lastScale = new();
        private readonly Dictionary<int, Vector3> _lastPosition = new();
        private readonly Dictionary<int, Quaternion> _lastRotation = new();

        private SamplePayloadPool<TransformUpdatePosition> _updatePositionPayloadPool;
        private SamplePayloadPool<TransformUpdateRotation> _updateRotationPayloadPool;
        private SamplePayloadPool<TransformUpdateScale> _updateScalePayloadPool;

        private int nUpdates = 0;
        private double time = 0;
        
        public void Start()
        {
            _updatePositionPayloadPool = recorder.GetSamplePoolManager().CreateSamplePayloadPool(() =>
            {
                var updatePositionSample = new TransformUpdatePosition();
                updatePositionSample.LocalPosition = new Sample.Common.Vector3();
                updatePositionSample.WorldPosition = new Sample.Common.Vector3();
                updatePositionSample.Id = new TransformGameObjectIdentifier();
                return updatePositionSample;
            });

            _updateRotationPayloadPool = recorder.GetSamplePoolManager().CreateSamplePayloadPool(() =>
            {
                var updateRotationSample = new TransformUpdateRotation();
                updateRotationSample.LocalRotation = new Sample.Common.Quaternion();
                updateRotationSample.WorldRotation = new Sample.Common.Quaternion();
                updateRotationSample.Id = new TransformGameObjectIdentifier();
                return updateRotationSample;
            });

            _updateScalePayloadPool = recorder.GetSamplePoolManager().CreateSamplePayloadPool(() =>
            {
                var updateScaleSample = new TransformUpdateScale();
                updateScaleSample.LocalScale = new Sample.Common.Vector3();
                updateScaleSample.WorldScale = new Sample.Common.Vector3();
                updateScaleSample.Id = new TransformGameObjectIdentifier();
                return updateScaleSample;
            });
        }

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

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            RecordUpdate();
            stopwatch.Stop();
            time += stopwatch.Elapsed.TotalMilliseconds;
            nUpdates++;

            if (nUpdates % 1000 == 0)
            {
                Debug.Log("Avg time: " + time / nUpdates + "ms");
            }
        }

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is Transform t and not RectTransform)
            {
                if (!_recordedTransforms.ContainsKey(t.GetInstanceID()))
                {
                    var transformId = t.GetInstanceID();
                    _recordedTransforms.Add(transformId, t);
                    _cachedIdentifiers.Add(transformId, new CachedTransformGameObjectIdentifier
                    {
                        TransformId = t.GetInstanceID().ToString(),
                        GameObjectId = t.gameObject.GetInstanceID().ToString()
                    });
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
            var transformCreate = new TransformCreate {Id = _cachedIdentifiers[transformId].ToPayload()};
            var transformUpdateParent = CreateTransformUpdateParent(t);
            var transformUpdateSiblingIndex = CreateTransformUpdateSiblingIndex(t);

            t.GetPositionAndRotation(out var position, out var rotation);
            var lossyScale = t.lossyScale;
            t.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
            var localScale = t.localScale;

            var transformUpdatePosition = new TransformUpdatePosition
            {
                Id = _cachedIdentifiers[transformId].ToPayload(),
                LocalPosition = new Sample.Common.Vector3
                    {X = localPosition.x, Y = localPosition.y, Z = localPosition.z},
                WorldPosition = new Sample.Common.Vector3 {X = position.x, Y = position.y, Z = position.z},
            };

            var transformUpdateRotation = new TransformUpdateRotation
            {
                Id = _cachedIdentifiers[transformId].ToPayload(),
                LocalRotation = new Sample.Common.Quaternion
                    {X = localRotation.x, Y = localRotation.y, Z = localRotation.z, W = localRotation.w},
                WorldRotation = new Sample.Common.Quaternion
                    {X = rotation.x, Y = rotation.y, Z = rotation.z, W = rotation.w},
            };

            var transformUpdateScale = new TransformUpdateScale
            {
                Id = _cachedIdentifiers[transformId].ToPayload(),
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
            var transformDestroy = new TransformDestroy {Id = _cachedIdentifiers[transformId].ToPayload()};
            recorder.RecordSample(transformDestroy);
        }

        private void RecordUpdate()
        {
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

                    var parentHasChanged = (parent == null && lastParentTransformId.HasValue) ||
                                           (parent != null && !lastParentTransformId.HasValue) ||
                                           (parent != null && lastParentTransformId.HasValue &&
                                            lastParentTransformId.Value != parent.GetInstanceID());

                    if (parentHasChanged)
                    {
                        var transformUpdateParent = CreateTransformUpdateParent(t);
                        recorder.RecordSample(transformUpdateParent);
                        _lastParentTransformId[transformId] = t.parent == null ? null : t.parent.GetInstanceID();
                    }
                    
                    t.GetPositionAndRotation(out var position, out var rotation);
                    var scale = t.lossyScale;

                    if (lastPosition != position && lastRotation != rotation)
                    {
                        t.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
                        
                        var cachedIdentifier = _cachedIdentifiers[transformId];

                        TransformUpdatePosition positionSample;
                        TransformUpdateRotation rotationSample;
                        
                        if (recorder.enableSamplePooling)
                        {
                            positionSample = _updatePositionPayloadPool.Get();
                            rotationSample = _updateRotationPayloadPool.Get();
                        }
                        else
                        {
                            positionSample = new TransformUpdatePosition();
                            positionSample.LocalPosition = new Sample.Common.Vector3();
                            positionSample.WorldPosition = new Sample.Common.Vector3();
                            positionSample.Id = new TransformGameObjectIdentifier();
                            
                            rotationSample = new TransformUpdateRotation();
                            rotationSample.LocalRotation = new Sample.Common.Quaternion();
                            rotationSample.WorldRotation = new Sample.Common.Quaternion();
                            rotationSample.Id = new TransformGameObjectIdentifier();
                        }

                        positionSample.Id.TransformId = cachedIdentifier.TransformId;
                        positionSample.Id.GameObjectId = cachedIdentifier.GameObjectId;
                        positionSample.LocalPosition.X = localPosition.x;
                        positionSample.LocalPosition.Y = localPosition.y;
                        positionSample.LocalPosition.Z = localPosition.z;
                        positionSample.WorldPosition.X = position.x;
                        positionSample.WorldPosition.Y = position.y;
                        positionSample.WorldPosition.Z = position.z;
                        
                        recorder.RecordSample(positionSample);

                        rotationSample.Id.TransformId = cachedIdentifier.TransformId;
                        rotationSample.Id.GameObjectId = cachedIdentifier.GameObjectId;
                        rotationSample.LocalRotation.X = localRotation.x;
                        rotationSample.LocalRotation.Y = localRotation.y;
                        rotationSample.LocalRotation.Z = localRotation.z;
                        rotationSample.LocalRotation.W = localRotation.w;
                        rotationSample.WorldRotation.X = rotation.x;
                        rotationSample.WorldRotation.Y = rotation.y;
                        rotationSample.WorldRotation.Z = rotation.z;
                        rotationSample.WorldRotation.W = rotation.w;

                        recorder.RecordSample(rotationSample);

                        _lastPosition[transformId] = position;
                        _lastRotation[transformId] = rotation;
                    }
                    else if (lastPosition != position)
                    {
                        var localPosition = t.localPosition;
                        
                        var cachedIdentifier = _cachedIdentifiers[transformId];

                        TransformUpdatePosition positionSample;
                        
                        if (recorder.enableSamplePooling)
                        {
                            positionSample = _updatePositionPayloadPool.Get();
                        }
                        else
                        {
                            positionSample = new TransformUpdatePosition();
                            positionSample.LocalPosition = new Sample.Common.Vector3();
                            positionSample.WorldPosition = new Sample.Common.Vector3();
                            positionSample.Id = new TransformGameObjectIdentifier();
                        }
                        
                        positionSample.Id.TransformId = cachedIdentifier.TransformId;
                        positionSample.Id.GameObjectId = cachedIdentifier.GameObjectId;
                        positionSample.LocalPosition.X = localPosition.x;
                        positionSample.LocalPosition.Y = localPosition.y;
                        positionSample.LocalPosition.Z = localPosition.z;
                        positionSample.WorldPosition.X = position.x;
                        positionSample.WorldPosition.Y = position.y;
                        positionSample.WorldPosition.Z = position.z;

                        recorder.RecordSample(positionSample);
                        
                        _lastPosition[transformId] = position;
                    }
                    else if (lastRotation != rotation)
                    {
                        var localRotation = t.localRotation;   
                        var cachedIdentifier = _cachedIdentifiers[transformId];

                        TransformUpdateRotation rotationSample;
                        
                        if (recorder.enableSamplePooling)
                        {
                            rotationSample = _updateRotationPayloadPool.Get();
                        }
                        else
                        {
                            rotationSample = new TransformUpdateRotation();
                            rotationSample.LocalRotation = new Sample.Common.Quaternion();
                            rotationSample.WorldRotation = new Sample.Common.Quaternion();
                            rotationSample.Id = new TransformGameObjectIdentifier();
                        }

                        rotationSample.Id.TransformId = cachedIdentifier.TransformId;
                        rotationSample.Id.GameObjectId = cachedIdentifier.GameObjectId;
                        rotationSample.LocalRotation.X = localRotation.x;
                        rotationSample.LocalRotation.Y = localRotation.y;
                        rotationSample.LocalRotation.Z = localRotation.z;
                        rotationSample.LocalRotation.W = localRotation.w;
                        rotationSample.WorldRotation.X = rotation.x;
                        rotationSample.WorldRotation.Y = rotation.y;
                        rotationSample.WorldRotation.Z = rotation.z;
                        rotationSample.WorldRotation.W = rotation.w;

                        recorder.RecordSample(rotationSample);

                        _lastRotation[transformId] = rotation;
                    }

                    if (lastScale != scale)
                    {
                        var localScale = t.localScale;
                        var cachedIdentifier = _cachedIdentifiers[transformId];
                        
                        TransformUpdateScale scaleSample;
                        
                        if (recorder.enableSamplePooling)
                        {
                            scaleSample = _updateScalePayloadPool.Get();
                        }
                        else
                        {
                            scaleSample = new TransformUpdateScale();
                            scaleSample.LocalScale = new Sample.Common.Vector3();
                            scaleSample.WorldScale = new Sample.Common.Vector3();
                            scaleSample.Id = new TransformGameObjectIdentifier();
                        }
                        
                        scaleSample.Id.TransformId = cachedIdentifier.TransformId;
                        scaleSample.Id.GameObjectId = cachedIdentifier.GameObjectId;
                        scaleSample.LocalScale.X = localScale.x;
                        scaleSample.LocalScale.Y = localScale.y;
                        scaleSample.LocalScale.Z = localScale.z;
                        scaleSample.WorldScale.X = scale.x;
                        scaleSample.WorldScale.Y = scale.y;
                        scaleSample.WorldScale.Z = scale.z;

                        recorder.RecordSample(scaleSample);
                        
                        _lastScale[transformId] = scale;
                    }

                    t.hasChanged = false;
                }

                var hasSiblingIndexChanged = _lastSiblingIndex[transformId] != t.GetSiblingIndex();

                if (hasSiblingIndexChanged)
                {
                    var transformUpdateSiblingIndex = CreateTransformUpdateSiblingIndex(t);
                    recorder.RecordSample(transformUpdateSiblingIndex);
                    _lastSiblingIndex[transformId] = t.GetSiblingIndex();
                }
            }

            foreach (var nullTransformInstanceId in nullTransformInstanceIds)
            {
                RecordDestruction(nullTransformInstanceId);
                RemoveFromCache(nullTransformInstanceId);
            }
        }

        private TransformUpdateSiblingIndex CreateTransformUpdateSiblingIndex(Transform t)
        {
            var transformId = t.GetInstanceID();

            var transformUpdateSiblingIndex = new TransformUpdateSiblingIndex
            {
                Id = new TransformGameObjectIdentifier
                {
                    TransformId = _cachedIdentifiers[transformId].TransformId,
                    GameObjectId = _cachedIdentifiers[transformId].GameObjectId
                },
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

                parentIdentifier = _cachedIdentifiers.TryGetValue(parentId, out var cachedIdentifier)
                    ? cachedIdentifier.ToPayload()
                    : parent.ToIdentifierPayload();
            }

            var transformId = t.GetInstanceID();

            var transformUpdateParent = new TransformUpdateParent
            {
                Id = _cachedIdentifiers[transformId].ToPayload(),
                ParentId = parentIdentifier
            };

            return transformUpdateParent;
        }
    }
}