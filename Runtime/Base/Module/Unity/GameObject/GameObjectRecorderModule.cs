using System.Collections.Generic;
using PLUME.Base.Hooks;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using static PLUME.Core.Utils.SampleUtils;
using GameObjectIdentifier = PLUME.Sample.ProtoBurst.Unity.GameObjectIdentifier;

namespace PLUME.Base.Module.Unity.GameObject
{
    [Preserve]
    public class GameObjectRecorderModule : ObjectRecorderModule<GameObjectIdentifier,
        GameObjectSafeRef, GameObjectFrameData>
    {
        private readonly Dictionary<GameObjectSafeRef, GameObjectCreate> _createSamples = new();
        private readonly Dictionary<GameObjectSafeRef, GameObjectDestroy> _destroySamples = new();
        private readonly Dictionary<GameObjectSafeRef, GameObjectUpdate> _updateSamples = new();

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            GameObjectHooks.OnCreated += go => OnCreated(go, ctx);
            GameObjectHooks.OnActiveChanged += (go, _) => OnActiveChanged(go, ctx);
            GameObjectHooks.OnTagChanged += (go, tag) => OnTagChanged(go, tag, ctx);
            ObjectHooks.OnNameChanged += (obj, name) => OnNameChanged(obj, name, ctx);
            ObjectHooks.OnBeforeDestroyed += (obj, _) => OnBeforeDestroyed(obj, ctx);
            SceneManagerHooks.OnGameObjectMovedToScene += (go, _, scene) => OnMoveGameObjectToScene(go, scene, ctx);
        }

        protected override void OnObjectMarkedCreated(GameObjectSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            _createSamples[objSafeRef] = new GameObjectCreate
            {
                Id = GetGameObjectIdentifierPayload(objSafeRef)
            };

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Active = objSafeRef.GameObject.activeSelf;
            updateSample.Name = objSafeRef.GameObject.name;
            updateSample.Layer = objSafeRef.GameObject.layer;
            updateSample.Tag = objSafeRef.GameObject.tag;
            updateSample.Scene = GetSceneIdentifierPayload(objSafeRef.SceneSafeRef);
        }

        protected override void OnObjectMarkedDestroyed(GameObjectSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new GameObjectDestroy { Id = GetGameObjectIdentifierPayload(objSafeRef) };
        }

        private void OnMoveGameObjectToScene(UnityEngine.GameObject go, UnityEngine.SceneManagement.Scene scene,
            RecorderContext ctx)
        {
            // We need to modify the scene safe ref of the game object
            var objSafeRef = ctx.SafeRefProvider.GetOrCreateGameObjectSafeRef(go);
            objSafeRef.SceneSafeRef = ctx.SafeRefProvider.GetOrCreateSceneSafeRef(scene);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Scene = GetSceneIdentifierPayload(objSafeRef.SceneSafeRef);
        }

        private void OnActiveChanged(UnityEngine.GameObject go, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.SafeRefProvider.GetOrCreateGameObjectSafeRef(go);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Active = go.activeSelf;
        }

        private void OnCreated(UnityEngine.GameObject go, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.SafeRefProvider.GetOrCreateGameObjectSafeRef(go);

            if (IsRecordingObject(objSafeRef))
                return;

            StartRecordingObject(objSafeRef, true, ctx);
        }

        private void OnTagChanged(UnityEngine.GameObject go, string tag, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.SafeRefProvider.GetOrCreateGameObjectSafeRef(go);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Tag = tag;
        }

        private void OnNameChanged(Object obj, string name, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            if (obj is not UnityEngine.GameObject go)
                return;

            var objSafeRef = ctx.SafeRefProvider.GetOrCreateGameObjectSafeRef(go);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Name = name;
        }

        private void OnBeforeDestroyed(Object obj, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            if (obj is not UnityEngine.GameObject go)
                return;

            var objSafeRef = ctx.SafeRefProvider.GetOrCreateGameObjectSafeRef(go);

            if (!IsRecordingObject(objSafeRef))
                return;

            StopRecordingObject(objSafeRef, true, ctx);
        }

        private GameObjectUpdate GetOrCreateUpdateSample(GameObjectSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new GameObjectUpdate { Id = GetGameObjectIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override GameObjectFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = GameObjectFrameData.Pool.Get();
            frameData.AddCreateSamples(_createSamples.Values);
            frameData.AddDestroySamples(_destroySamples.Values);
            frameData.AddUpdateSamples(_updateSamples.Values);
            return frameData;
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
        }
    }
}