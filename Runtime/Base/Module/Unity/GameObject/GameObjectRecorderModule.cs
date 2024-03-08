using System.Collections.Generic;
using PLUME.Base.Events;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using static PLUME.Core.Utils.SampleUtils;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Scripting;
using GameObjectIdentifier = PLUME.Core.Object.GameObjectIdentifier;

namespace PLUME.Base.Module.Unity.GameObject
{
    [Preserve]
    public class GameObjectRecorderModule : ObjectRecorderModule<UnityEngine.GameObject, GameObjectIdentifier,
        GameObjectSafeRef, GameObjectFrameData>
    {
        private readonly Dictionary<GameObjectSafeRef, GameObjectCreate> _createSamples = new();
        private readonly Dictionary<GameObjectSafeRef, GameObjectDestroy> _destroySamples = new();
        private readonly Dictionary<GameObjectSafeRef, GameObjectUpdate> _updateSamples = new();

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            GameObjectEvents.OnCreated += go => OnCreated(go, ctx);
            GameObjectEvents.OnActiveChanged += (go, _) => OnActiveChanged(go, ctx);
            GameObjectEvents.OnTagChanged += (go, tag) => OnTagChanged(go, tag, ctx);
            ObjectEvents.OnNameChanged += (obj, name) => OnNameChanged(obj, name, ctx);
            ObjectEvents.OnBeforeDestroyed += (obj, _) => OnBeforeDestroyed(obj, ctx);
        }

        protected override void OnObjectMarkedCreated(GameObjectSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Active = objSafeRef.GameObject.activeSelf;
            updateSample.Name = objSafeRef.GameObject.name;
            updateSample.Layer = objSafeRef.GameObject.layer;
            updateSample.Tag = objSafeRef.GameObject.tag;
            _createSamples[objSafeRef] = new GameObjectCreate { Id = GetGameObjectIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(GameObjectSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new GameObjectDestroy { Id = GetGameObjectIdentifierPayload(objSafeRef) };
        }

        private void OnActiveChanged(UnityEngine.GameObject go, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateGameObjectSafeRef(go);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Active = go.activeSelf;
        }

        private void OnCreated(UnityEngine.GameObject go, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateGameObjectSafeRef(go);

            if (IsRecordingObject(objSafeRef))
                return;

            StartRecordingObject(objSafeRef, true, ctx);
        }

        private void OnTagChanged(UnityEngine.GameObject go, string tag, RecorderContext ctx)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateGameObjectSafeRef(go);

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

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateGameObjectSafeRef(go);

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

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateGameObjectSafeRef(go);

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