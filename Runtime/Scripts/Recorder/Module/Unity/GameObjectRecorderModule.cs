using System.Collections.Generic;
using PLUME.Guid;
using PLUME.Sample.Unity;
using UnityEngine;

namespace PLUME
{
    public class GameObjectRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver,
        IStartRecordingEventReceiver,
        IStopRecordingEventReceiver
    {
        private readonly Dictionary<int, GameObject> _recordedGameObjects = new();
        private readonly Dictionary<int, string> _cachedGameObjectIdentifiers = new();
        private readonly Dictionary<int, string> _cachedTransformIdentifiers = new();
        private readonly Dictionary<int, bool> _lastGameObjectActive = new();

        protected override void ResetCache()
        {
            _recordedGameObjects.Clear();
            _lastGameObjectActive.Clear();
            _cachedGameObjectIdentifiers.Clear();
            _cachedTransformIdentifiers.Clear();
        }

        public new void OnStartRecording()
        {
            base.OnStartRecording();
            ObjectEvents.OnSetName += OnObjectSetNameCallback;
        }

        public void OnStopRecording()
        {
            ObjectEvents.OnSetName -= OnObjectSetNameCallback;
        }

        public void OnStartRecordingObject(Object obj)
        {
            if (obj is not GameObject go) return;
            if (_recordedGameObjects.ContainsKey(go.GetInstanceID())) return;

            var goInstanceId = go.GetInstanceID();

            var guidRegistry = SceneObjectsGuidRegistry.GetOrCreateInScene(go.scene);
            var gameObjectGuidRegistryEntry = guidRegistry.GetOrCreate(go);
            var transformGuidRegistryEntry = guidRegistry.GetOrCreate(go.transform);

            _recordedGameObjects.Add(goInstanceId, go);
            _cachedGameObjectIdentifiers.Add(goInstanceId, gameObjectGuidRegistryEntry.guid);
            _cachedTransformIdentifiers.Add(goInstanceId, transformGuidRegistryEntry.guid);
            _lastGameObjectActive.Add(goInstanceId, go.activeSelf);
            RecordCreation(go);
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            if (!_recordedGameObjects.ContainsKey(objectInstanceId)) return;
            RecordDestruction(objectInstanceId);
            RemoveFromCache(objectInstanceId);
        }

        private void OnObjectSetNameCallback(Object obj, string value)
        {
            if (obj is GameObject go)
            {
                var goInstanceId = go.GetInstanceID();

                if (_recordedGameObjects.ContainsKey(goInstanceId))
                {
                    var gameObjectUpdateName = new GameObjectUpdateName
                    {
                        Id = go.ToIdentifierPayload(),
                        Name = go.name
                    };

                    recorder.RecordSample(gameObjectUpdateName);
                }
            }
        }

        private void FixedUpdate()
        {
            var nullGameObjectInstanceIds = new List<int>();

            foreach (var (gameObjectInstanceId, go) in _recordedGameObjects)
            {
                if (go == null)
                {
                    nullGameObjectInstanceIds.Add(gameObjectInstanceId);
                    RecordDestruction(gameObjectInstanceId);
                    continue;
                }

                if (_lastGameObjectActive[gameObjectInstanceId] != go.activeSelf)
                {
                    _lastGameObjectActive[gameObjectInstanceId] = go.activeSelf;

                    var gameObjectUpdateActiveSelf = new GameObjectUpdateActiveSelf
                    {
                        Id = go.ToIdentifierPayload(),
                        Active = go.activeSelf
                    };

                    recorder.RecordSample(gameObjectUpdateActiveSelf);
                }
            }

            foreach (var nullGoInstanceId in nullGameObjectInstanceIds)
            {
                RemoveFromCache(nullGoInstanceId);
            }
        }

        private void RemoveFromCache(int gameObjectInstanceId)
        {
            _recordedGameObjects.Remove(gameObjectInstanceId);
            _lastGameObjectActive.Remove(gameObjectInstanceId);
            _cachedTransformIdentifiers.Remove(gameObjectInstanceId);
            _cachedGameObjectIdentifiers.Remove(gameObjectInstanceId);
        }

        private void RecordCreation(GameObject go)
        {
            var gameObjectCreation = new GameObjectCreate { Id = go.ToIdentifierPayload() };

            var gameObjectUpdateName = new GameObjectUpdateName
            {
                Id = go.ToIdentifierPayload(),
                Name = go.name
            };

            var gameObjectUpdateLayer = new GameObjectUpdateLayer
            {
                Id = go.ToIdentifierPayload(),
                Layer = go.layer
            };

            var gameObjectUpdateTag = new GameObjectUpdateTag
            {
                Id = go.ToIdentifierPayload(),
                Tag = go.tag
            };

            var gameObjectUpdateScene = new GameObjectUpdateScene
            {
                Id = go.ToIdentifierPayload(),
                SceneId = go.scene.buildIndex
            };

            var gameObjectUpdateActiveSelf = new GameObjectUpdateActiveSelf
            {
                Id = go.ToIdentifierPayload(),
                Active = go.activeSelf
            };

            recorder.RecordSample(gameObjectCreation);
            recorder.RecordSample(gameObjectUpdateName);
            recorder.RecordSample(gameObjectUpdateLayer);
            recorder.RecordSample(gameObjectUpdateTag);
            recorder.RecordSample(gameObjectUpdateScene);
            recorder.RecordSample(gameObjectUpdateActiveSelf);
        }

        private void RecordDestruction(int gameObjectInstanceId)
        {
            var gameObjectDestroy = new GameObjectDestroy
            {
                Id = new TransformGameObjectIdentifier
                {
                    TransformId = _cachedTransformIdentifiers[gameObjectInstanceId],
                    GameObjectId = _cachedGameObjectIdentifiers[gameObjectInstanceId]
                }
            };
            recorder.RecordSample(gameObjectDestroy);
        }
    }
}