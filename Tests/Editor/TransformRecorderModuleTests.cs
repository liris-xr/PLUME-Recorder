using System.Collections.Generic;
using NUnit.Framework;
using PLUME.Base.Hooks;
using PLUME.Base.Module.Unity.Transform;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Settings;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.Editor
{
    /// <summary>
    /// The module keeps transforms, their states and an identifier-to-index map aligned: stopping and recording the
    /// same transform again must keep them so.
    /// </summary>
    [TestFixture]
    public class TransformRecorderModuleTests
    {
        private readonly List<GameObject> _created = new();

        private TransformRecorderModule _module;
        private RecorderContext _ctx;
        private NativeList<byte> _out;
        private int _frame;

        [SetUp]
        public void SetUp()
        {
            _module = new TransformRecorderModule();
            var modules = new List<IRecorderModule> { _module }.AsReadOnly();
            _ctx = new RecorderContext(modules, new SafeRefProvider(), new FileSettingsProvider());
            ((IRecorderModule)_module).Create(_ctx);
            ((IRecorderModule)_module).StartRecording(_ctx);
            _ctx.Status = RecorderStatus.Recording;
            _out = new NativeList<byte>(Allocator.Persistent);
            _frame = 0;
        }

        [TearDown]
        public void TearDown()
        {
            ((IRecorderModule)_module).StopRecording(_ctx);
            _ctx.Status = RecorderStatus.Stopped;
            ((IRecorderModule)_module).Destroy(_ctx);
            _out.Dispose();
            foreach (var go in _created)
                Object.DestroyImmediate(go);
            _created.Clear();
        }

        [TestCase(0)]
        [TestCase(2)]
        public void Transform_RecordedAgainAfterStopping_IsRecorded(int stoppedIndex)
        {
            var transforms = new IComponentSafeRef<Transform>[3];
            for (var i = 0; i < transforms.Length; i++)
            {
                var go = new GameObject("t" + i);
                _created.Add(go);
                transforms[i] = _ctx.SafeRefProvider.GetOrCreateComponentSafeRef(go.transform);
                Start(transforms[i]);
            }
            RunFrame();

            var stopped = transforms[stoppedIndex];
            ((IObjectRecorderModule)_module).StopRecordingObject(stopped, false, _ctx);
            RunFrame();
            Start(stopped);
            stopped.Component.localPosition = Vector3.one;
            RunFrame();

            foreach (var t in transforms)
                Assert.IsTrue(_module.IsRecordingObject(t));
        }

        [Test]
        public void Transform_SharesItsGuidWithItsGameObjectIdentifier()
        {
            var go = new GameObject("runtime");
            _created.Add(go);

            var transformGuid = _ctx.SafeRefProvider.GetOrCreateComponentSafeRef(go.transform).Identifier.Guid;

            Assert.AreEqual(_ctx.SafeRefProvider.GetOrCreateGameObjectSafeRef(go).Identifier.TransformGuid,
                transformGuid);
        }

        // Unity replaces the Transform by a RectTransform when a UI component is added. A replaced Transform left in a
        // TransformAccessArray crashes Unity when its GameObject is destroyed (here, in TearDown).
        [Test]
        public void Transform_ReplacedByRectTransform_IsRecordedAsTheSameTransform()
        {
            var go = new GameObject("replaced");
            _created.Add(go);
            var transform = _ctx.SafeRefProvider.GetOrCreateComponentSafeRef(go.transform);
            Start(transform);
            RunFrame();

            GameObjectHooks.AddComponentAndNotify<Image>(go);
            RunFrame();

            var rectTransform = _ctx.SafeRefProvider.GetOrCreateComponentSafeRef(go.transform);
            Assert.IsInstanceOf<RectTransform>(rectTransform.Component);
            Assert.IsTrue(_module.IsRecordingObject(rectTransform));
            Assert.IsFalse(_module.IsRecordingObject(transform));
            Assert.AreEqual(transform.Identifier.Guid, rectTransform.Identifier.Guid);
        }

        private void Start(IComponentSafeRef<Transform> t)
        {
            Assert.IsTrue(((IObjectRecorderModule)_module).StartRecordingObject(t, false, _ctx));
        }

        private void RunFrame()
        {
            var frameInfo = new FrameInfo((ulong)_frame, _frame++);
            var module = (IFrameDataRecorderModule)_module;
            module.BeforeEnqueueFrameData(frameInfo, _ctx);
            module.EnqueueFrameData(frameInfo, _ctx);
            module.AfterEnqueueFrameData(frameInfo, _ctx);
            module.SerializeFrameData(frameInfo, new FrameDataWriter(_out));
        }
    }
}
