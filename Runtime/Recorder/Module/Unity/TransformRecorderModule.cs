using System.Collections.Generic;
using Google.Protobuf;
using PLUME.Sample;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Profiling;
using UnityRuntimeGuid;

namespace PLUME
{
    [DisallowMultipleComponent]
    public class TransformRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, Transform> _recordedTransforms = new();
        private readonly Dictionary<int, string> _cachedGameObjectIdentifiers = new();
        private readonly Dictionary<int, string> _cachedTransformIdentifiers = new();

        private readonly Dictionary<int, int> _lastSiblingIndex = new();
        private readonly Dictionary<int, int?> _lastParentTransformId = new();
        private readonly Dictionary<int, Vector3> _lastScale = new();
        private readonly Dictionary<int, Vector3> _lastPosition = new();
        private readonly Dictionary<int, Quaternion> _lastRotation = new();

        private SamplePayloadPool<TransformUpdatePosition> _updatePositionPayloadPool;
        private SamplePayloadPool<TransformUpdateRotation> _updateRotationPayloadPool;
        private SamplePayloadPool<TransformUpdateScale> _updateScalePayloadPool;
        
        public void Start()
        {
            _updatePositionPayloadPool = recorder.GetSamplePoolManager().CreateSamplePayloadPool(() =>
            {
                var updatePositionSample = new TransformUpdatePosition();
                updatePositionSample.LocalPosition = new Sample.Common.Vector3();
                updatePositionSample.WorldPosition = new Sample.Common.Vector3();
                updatePositionSample.Id = new TransformGameObjectIdentifier();
                return updatePositionSample;
            }, 1000);

            _updateRotationPayloadPool = recorder.GetSamplePoolManager().CreateSamplePayloadPool(() =>
            {
                var updateRotationSample = new TransformUpdateRotation();
                updateRotationSample.LocalRotation = new Sample.Common.Quaternion();
                updateRotationSample.WorldRotation = new Sample.Common.Quaternion();
                updateRotationSample.Id = new TransformGameObjectIdentifier();
                return updateRotationSample;
            }, 1000);

            _updateScalePayloadPool = recorder.GetSamplePoolManager().CreateSamplePayloadPool(() =>
            {
                var updateScaleSample = new TransformUpdateScale();
                updateScaleSample.LocalScale = new Sample.Common.Vector3();
                updateScaleSample.WorldScale = new Sample.Common.Vector3();
                updateScaleSample.Id = new TransformGameObjectIdentifier();
                return updateScaleSample;
            }, 1000);
        }

        protected override void ResetCache()
        {
            _recordedTransforms.Clear();
            _cachedGameObjectIdentifiers.Clear();
            _cachedTransformIdentifiers.Clear();
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
            if (obj is not (Transform t and not RectTransform)) return;
            if (_recordedTransforms.ContainsKey(t.GetInstanceID())) return;
            
            var guidRegistry = SceneGuidRegistry.GetOrCreate(t.gameObject.scene);
            var gameObjectGuidRegistryEntry = guidRegistry.GetOrCreateEntry(t.gameObject);
            var transformGuidRegistryEntry = guidRegistry.GetOrCreateEntry(t.gameObject.transform);
            var transformId = t.GetInstanceID();
            
            _recordedTransforms.Add(transformId, t);
            _cachedGameObjectIdentifiers.Add(transformId, gameObjectGuidRegistryEntry.guid);
            _cachedTransformIdentifiers.Add(transformId, transformGuidRegistryEntry.guid);
            _lastSiblingIndex.Add(transformId, t.GetSiblingIndex());
            _lastParentTransformId.Add(transformId, t.parent == null ? null : t.parent.GetInstanceID());
            _lastScale.Add(transformId, t.transform.lossyScale);
            _lastPosition.Add(transformId, t.transform.position);
            _lastRotation.Add(transformId, t.transform.rotation);
            RecordCreation(t);
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (!_recordedTransforms.TryGetValue(objectInstanceId, out var t)) return;
            if (t == null) return;
            RecordDestruction(objectInstanceId);
            RemoveFromCache(objectInstanceId);
        }

        private void RemoveFromCache(int transformInstanceId)
        {
            _recordedTransforms.Remove(transformInstanceId);
            _lastSiblingIndex.Remove(transformInstanceId);
            _lastParentTransformId.Remove(transformInstanceId);
            _lastScale.Remove(transformInstanceId);
            _lastPosition.Remove(transformInstanceId);
            _lastRotation.Remove(transformInstanceId);
            _cachedGameObjectIdentifiers.Remove(transformInstanceId);
            _cachedTransformIdentifiers.Remove(transformInstanceId);
        }

        private void RecordCreation(Transform t)
        {
            var transformId = t.GetInstanceID();
            var transformCreate = new TransformCreate {Id = new TransformGameObjectIdentifier
            {
                TransformId = _cachedTransformIdentifiers[transformId],
                GameObjectId = _cachedGameObjectIdentifiers[transformId]
            }};
            var transformUpdateParent = CreateTransformUpdateParent(t);
            var transformUpdateSiblingIndex = CreateTransformUpdateSiblingIndex(t);

            t.GetPositionAndRotation(out var position, out var rotation);
            var lossyScale = t.lossyScale;
            t.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
            var localScale = t.localScale;

            var transformUpdatePosition = new TransformUpdatePosition
            {
                Id = new TransformGameObjectIdentifier
                {
                    TransformId = _cachedTransformIdentifiers[transformId],
                    GameObjectId = _cachedGameObjectIdentifiers[transformId]
                },
                LocalPosition = new Sample.Common.Vector3
                    {X = localPosition.x, Y = localPosition.y, Z = localPosition.z},
                WorldPosition = new Sample.Common.Vector3 {X = position.x, Y = position.y, Z = position.z},
            };

            var transformUpdateRotation = new TransformUpdateRotation
            {
                Id = new TransformGameObjectIdentifier
                {
                    TransformId = _cachedTransformIdentifiers[transformId],
                    GameObjectId = _cachedGameObjectIdentifiers[transformId]
                },
                LocalRotation = new Sample.Common.Quaternion
                    {X = localRotation.x, Y = localRotation.y, Z = localRotation.z, W = localRotation.w},
                WorldRotation = new Sample.Common.Quaternion
                    {X = rotation.x, Y = rotation.y, Z = rotation.z, W = rotation.w},
            };

            var transformUpdateScale = new TransformUpdateScale
            {
                Id = new TransformGameObjectIdentifier
                {
                    TransformId = _cachedTransformIdentifiers[transformId],
                    GameObjectId = _cachedGameObjectIdentifiers[transformId]
                },
                LocalScale = new Sample.Common.Vector3 {X = localScale.x, Y = localScale.y, Z = localScale.z},
                WorldScale = new Sample.Common.Vector3 {X = lossyScale.x, Y = lossyScale.y, Z = lossyScale.z}
            };

            recorder.RecordSampleStamped(transformCreate);
            recorder.RecordSampleStamped(transformUpdateParent);
            recorder.RecordSampleStamped(transformUpdateSiblingIndex);
            recorder.RecordSampleStamped(transformUpdatePosition);
            recorder.RecordSampleStamped(transformUpdateRotation);
            recorder.RecordSampleStamped(transformUpdateScale);
        }

        private void RecordDestruction(int transformId)
        {
            var transformDestroy = new TransformDestroy {Id = new TransformGameObjectIdentifier
            {
                TransformId = _cachedTransformIdentifiers[transformId],
                GameObjectId = _cachedGameObjectIdentifiers[transformId]
            }};
            recorder.RecordSampleStamped(transformDestroy);
        }

        private void RecordUpdate()
        {
            // Keep track of any transforms destroyed (ref == null) that were not picked up by the system.
            // This can happen if the object is not destroyed by calling Destroy or DestroyImmediate (in Editor or internal C++ engine)
            var nullTransformInstanceIds = new List<int>();

            var recordedSamples = new List<UnpackedSample>();
            Vector3 position;
            Vector3 localPosition;
            Quaternion rotation;
            Quaternion localRotation;

            Profiler.BeginSample("Loop");
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
                        recordedSamples.Add(recorder.GetUnpackedSampleStamped(transformUpdateParent));
                        _lastParentTransformId[transformId] = t.parent == null ? null : t.parent.GetInstanceID();
                    }
                    
                    Profiler.BeginSample("GetPositionAndRotation");
                    t.GetPositionAndRotation(out position, out rotation);
                    var scale = t.lossyScale;
                    Profiler.EndSample();

                    if (lastPosition != position && lastRotation != rotation)
                    {
                        t.GetLocalPositionAndRotation(out localPosition, out localRotation);
                        
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

                        positionSample.Id.TransformId = _cachedTransformIdentifiers[transformId];
                        positionSample.Id.GameObjectId = _cachedGameObjectIdentifiers[transformId];
                        positionSample.LocalPosition.X = localPosition.x;
                        positionSample.LocalPosition.Y = localPosition.y;
                        positionSample.LocalPosition.Z = localPosition.z;
                        positionSample.WorldPosition.X = position.x;
                        positionSample.WorldPosition.Y = position.y;
                        positionSample.WorldPosition.Z = position.z;

                        recordedSamples.Add(recorder.GetUnpackedSampleStamped(positionSample));

                        rotationSample.Id.TransformId = _cachedTransformIdentifiers[transformId];
                        rotationSample.Id.GameObjectId = _cachedGameObjectIdentifiers[transformId];
                        rotationSample.LocalRotation.X = localRotation.x;
                        rotationSample.LocalRotation.Y = localRotation.y;
                        rotationSample.LocalRotation.Z = localRotation.z;
                        rotationSample.LocalRotation.W = localRotation.w;
                        rotationSample.WorldRotation.X = rotation.x;
                        rotationSample.WorldRotation.Y = rotation.y;
                        rotationSample.WorldRotation.Z = rotation.z;
                        rotationSample.WorldRotation.W = rotation.w;

                        recordedSamples.Add(recorder.GetUnpackedSampleStamped(rotationSample));

                        lastPosition.x = position.x;
                        lastPosition.y = position.y;
                        lastPosition.z = position.z;
                        lastRotation.x = lastRotation.x;
                        lastRotation.y = lastRotation.y;
                        lastRotation.z = lastRotation.z;
                        lastRotation.w = lastRotation.w;
                    }
                    else if (lastPosition != position)
                    {
                        Profiler.BeginSample("PositionChange");
                        localPosition = t.localPosition;
                        
                        TransformUpdatePosition positionSample;
                        
                        if (recorder.enableSamplePooling)
                        {
                            Profiler.BeginSample("GetFromPool");
                            positionSample = _updatePositionPayloadPool.Get();
                            Profiler.EndSample();
                        }
                        else
                        {
                            Profiler.BeginSample("CreateNew");
                            positionSample = new TransformUpdatePosition();
                            positionSample.LocalPosition = new Sample.Common.Vector3();
                            positionSample.WorldPosition = new Sample.Common.Vector3();
                            positionSample.Id = new TransformGameObjectIdentifier();
                            Profiler.EndSample();
                        }
                        
                        positionSample.Id.TransformId = _cachedTransformIdentifiers[transformId];
                        positionSample.Id.GameObjectId = _cachedGameObjectIdentifiers[transformId];
                        positionSample.LocalPosition.X = localPosition.x;
                        positionSample.LocalPosition.Y = localPosition.y;
                        positionSample.LocalPosition.Z = localPosition.z;
                        positionSample.WorldPosition.X = position.x;
                        positionSample.WorldPosition.Y = position.y;
                        positionSample.WorldPosition.Z = position.z;

                        Profiler.BeginSample("CreateSample");
                        recordedSamples.Add(recorder.GetUnpackedSampleStamped(positionSample));
                        lastPosition.x = position.x;
                        lastPosition.y = position.y;
                        lastPosition.z = position.z;
                        Profiler.EndSample();
                        
                        Profiler.EndSample();
                    }
                    else if (lastRotation != rotation)
                    {
                        localRotation = t.localRotation;   

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

                        rotationSample.Id.TransformId = _cachedTransformIdentifiers[transformId];
                        rotationSample.Id.GameObjectId = _cachedGameObjectIdentifiers[transformId];
                        rotationSample.LocalRotation.X = localRotation.x;
                        rotationSample.LocalRotation.Y = localRotation.y;
                        rotationSample.LocalRotation.Z = localRotation.z;
                        rotationSample.LocalRotation.W = localRotation.w;
                        rotationSample.WorldRotation.X = rotation.x;
                        rotationSample.WorldRotation.Y = rotation.y;
                        rotationSample.WorldRotation.Z = rotation.z;
                        rotationSample.WorldRotation.W = rotation.w;

                        recordedSamples.Add(recorder.GetUnpackedSampleStamped(rotationSample));

                        lastRotation.x = rotation.x;
                        lastRotation.y = rotation.y;
                        lastRotation.z = rotation.z;
                        lastRotation.w = rotation.w;
                    }

                    if (lastScale != scale)
                    {
                        var localScale = t.localScale;
                        
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
                        
                        scaleSample.Id.TransformId = _cachedTransformIdentifiers[transformId];
                        scaleSample.Id.GameObjectId = _cachedGameObjectIdentifiers[transformId];
                        scaleSample.LocalScale.X = localScale.x;
                        scaleSample.LocalScale.Y = localScale.y;
                        scaleSample.LocalScale.Z = localScale.z;
                        scaleSample.WorldScale.X = scale.x;
                        scaleSample.WorldScale.Y = scale.y;
                        scaleSample.WorldScale.Z = scale.z;

                        recordedSamples.Add(recorder.GetUnpackedSampleStamped(scaleSample));
                        
                        lastScale.x = scale.x;
                        lastScale.y = scale.y;
                        lastScale.z = scale.z;
                    }

                    t.hasChanged = false;
                }

                var hasSiblingIndexChanged = _lastSiblingIndex[transformId] != t.GetSiblingIndex();

                if (hasSiblingIndexChanged)
                {
                    var transformUpdateSiblingIndex = CreateTransformUpdateSiblingIndex(t);
                    recordedSamples.Add(recorder.GetUnpackedSampleStamped(transformUpdateSiblingIndex));
                    _lastSiblingIndex[transformId] = t.GetSiblingIndex();
                }
            }
            Profiler.EndSample();
            
            recorder.RecordUnpackedSamples(recordedSamples);

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
                    TransformId = _cachedTransformIdentifiers[transformId],
                    GameObjectId = _cachedGameObjectIdentifiers[transformId]
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

                if (_cachedGameObjectIdentifiers.TryGetValue(parentId, out var cachedGameObjectIdentifier)
                    && _cachedTransformIdentifiers.TryGetValue(parentId, out var cachedTransformIdentifier))
                {
                    parentIdentifier = new TransformGameObjectIdentifier
                    {
                        TransformId = cachedTransformIdentifier,
                        GameObjectId = cachedGameObjectIdentifier,
                    };
                }
                else
                {
                    parentIdentifier = parent.ToIdentifierPayload();
                }
            }

            var transformId = t.GetInstanceID();

            var transformUpdateParent = new TransformUpdateParent
            {
                Id = new TransformGameObjectIdentifier
                {
                    GameObjectId = _cachedGameObjectIdentifiers[transformId],
                    TransformId = _cachedTransformIdentifiers[transformId]
                },
                ParentId = parentIdentifier
            };

            return transformUpdateParent;
        }
    }
}