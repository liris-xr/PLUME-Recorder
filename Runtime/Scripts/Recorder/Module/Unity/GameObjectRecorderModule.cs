using System.Collections.Generic;
using PLUME.Sample.Unity;
using UnityEngine;

namespace PLUME
{
    public class GameObjectRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly Dictionary<int, GameObject> _recordedGameObjects = new();
        private readonly Dictionary<int, TransformGameObjectIdentifier> _cachedIdentifiers = new();
        private readonly Dictionary<int, bool> _lastGameObjectActive = new();

        protected override void ResetCache()
        {
            _recordedGameObjects.Clear();
            _cachedIdentifiers.Clear();
            _lastGameObjectActive.Clear();
        }

        public void OnStartRecordingObject(Object obj)
        {
            ObjectEvents.OnSetName += OnObjectSetNameCallback;

            if (obj is GameObject go)
            {
                var goInstanceId = go.GetInstanceID();

                if (!_recordedGameObjects.ContainsKey(goInstanceId))
                {
                    _recordedGameObjects.Add(goInstanceId, go);
                    _cachedIdentifiers.Add(goInstanceId, go.ToIdentifierPayload());
                    _lastGameObjectActive.Add(goInstanceId, go.activeSelf);
                    RecordCreation(go);
                }
            }
        }

        public void OnStopRecordingObject(int objectInstanceId)
        {
            ObjectEvents.OnSetName -= OnObjectSetNameCallback;

            if (_recordedGameObjects.ContainsKey(objectInstanceId))
            {
                RecordDestruction(objectInstanceId);
                RemoveFromCache(objectInstanceId);
            }
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
            _cachedIdentifiers.Remove(gameObjectInstanceId);
            _lastGameObjectActive.Remove(gameObjectInstanceId);
        }

        private void RecordCreation(GameObject go)
        {
            var gameObjectCreation = new GameObjectCreate {Id = go.ToIdentifierPayload()};

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
            var gameObjectDestroy = new GameObjectDestroy {Id = _cachedIdentifiers[gameObjectInstanceId]};
            recorder.RecordSample(gameObjectDestroy);
        }
    }
}