using System.Collections.Generic;
using PLUME.Sample.Unity;
using Runtime;
using UnityEngine;

namespace PLUME
{
    public class GameObjectRecorderModule : RecorderModule,
        IStartRecordingObjectEventReceiver,
        IStopRecordingObjectEventReceiver
    {
        private readonly List<ObjectNullSafeReference<GameObject>> _recordedGameObjectsRefs = new();
        private readonly Dictionary<ObjectNullSafeReference<GameObject>, TransformGameObjectIdentifier>
            _cachedIdentifiers = new();
        private readonly Dictionary<ObjectNullSafeReference<GameObject>, bool> _lastGameObjectActive = new();

        public void OnStartRecordingObject(Object obj)
        {
            ObjectEvents.OnSetName += OnObjectSetNameCallback;

            if (obj is GameObject go)
            {
                var goRef = new ObjectNullSafeReference<GameObject>(go);

                if (!_recordedGameObjectsRefs.Contains(goRef))
                {
                    _recordedGameObjectsRefs.Add(goRef);
                    _cachedIdentifiers.Add(goRef, go.ToIdentifierPayload());
                    _lastGameObjectActive.Add(goRef, go.activeSelf);
                    RecordCreation(goRef);
                }
            }
        }

        public void OnStopRecordingObject(Object obj)
        {
            ObjectEvents.OnSetName -= OnObjectSetNameCallback;

            if (obj is GameObject go)
            {
                var goRef = new ObjectNullSafeReference<GameObject>(go);
                
                if (_recordedGameObjectsRefs.Contains(goRef))
                {
                    RecordDestruction(goRef);
                    _recordedGameObjectsRefs.Remove(goRef);
                    _cachedIdentifiers.Remove(goRef);
                    _lastGameObjectActive.Remove(goRef);
                }
            }
        }

        private void OnObjectSetNameCallback(Object obj, string value)
        {
            if (obj is GameObject go)
            {
                var goRef = new ObjectNullSafeReference<GameObject>(go);
                
                if (_recordedGameObjectsRefs.Contains(goRef))
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
            var nullGameObjectRefs = new List<ObjectNullSafeReference<GameObject>>();

            foreach (var goRef in _recordedGameObjectsRefs)
            {
                if (goRef.HasBeenDestroyed())
                {
                    nullGameObjectRefs.Add(goRef);
                    RecordDestruction(goRef);
                    continue;
                }

                var go = goRef.Object;
                
                if (_lastGameObjectActive[goRef] != go.activeSelf)
                {
                    _lastGameObjectActive[goRef] = go.activeSelf;
                    
                    var gameObjectUpdateActiveSelf = new GameObjectUpdateActiveSelf
                    {
                        Id = go.ToIdentifierPayload(),
                        Active = go.activeSelf
                    };
                    
                    recorder.RecordSample(gameObjectUpdateActiveSelf);
                }
            }
            
            foreach (var nullGoRef in nullGameObjectRefs)
            {
                _recordedGameObjectsRefs.Remove(nullGoRef);
            }
        }

        private void RecordCreation(ObjectNullSafeReference<GameObject> goRef)
        {
            var go = goRef.Object;
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

        private void RecordDestruction(ObjectNullSafeReference<GameObject> goRef)
        {
            var gameObjectDestroy = new GameObjectDestroy { Id = _cachedIdentifiers[goRef] };
            recorder.RecordSample(gameObjectDestroy);
        }
    }
}