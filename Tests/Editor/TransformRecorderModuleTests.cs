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

            var warnings = CountWarnings(() =>
            {
                GameObjectHooks.AddComponentAndNotify<Image>(go);
                RunFrame();
            });

            var rectTransform = _ctx.SafeRefProvider.GetOrCreateComponentSafeRef(go.transform);
            Assert.IsInstanceOf<RectTransform>(rectTransform.Component);
            Assert.IsTrue(_module.IsRecordingObject(rectTransform));
            Assert.IsFalse(_module.IsRecordingObject(transform));
            Assert.AreEqual(transform.Identifier.Guid, rectTransform.Identifier.Guid);
            Assert.AreEqual(0, warnings, "A replacement seen by the hooks must not be reported.");
        }

        [Test]
        public void Transform_DestroyedWithItsGameObject_IsNotReportedAsReplaced()
        {
            var go = new GameObject("destroyed");
            Start(_ctx.SafeRefProvider.GetOrCreateComponentSafeRef(go.transform));
            RunFrame();

            var warnings = CountWarnings(() =>
            {
                Object.DestroyImmediate(go);
                RunFrame();
            });

            Assert.AreEqual(0, warnings);
        }

        [Test]
        public void Transform_DestroyedWithoutHooks_StopsBeingRecorded()
        {
            var go = new GameObject("destroyed unhooked");
            var transform = _ctx.SafeRefProvider.GetOrCreateComponentSafeRef(go.transform);
            Start(transform);
            RunFrame();

            // The hook handlers ignore what happens while not recording, as for code the hooks do not see.
            _ctx.Status = RecorderStatus.Stopped;
            Object.DestroyImmediate(go);
            _ctx.Status = RecorderStatus.Recording;

            RunFrame(); // Notices the destroyed transform.
            RunFrame(); // Stops recording it, marked destroyed.

            Assert.IsFalse(_module.IsRecordingObject(transform));
        }

        private static int CountWarnings(System.Action action)
        {
            var warnings = 0;
            void OnLog(string message, string stackTrace, LogType type)
            {
                if (type == LogType.Warning)
                    warnings++;
            }

            Application.logMessageReceived += OnLog;
            try
            {
                action();
            }
            finally
            {
                Application.logMessageReceived -= OnLog;
            }

            return warnings;
        }

        private void Start(IComponentSafeRef<Transform> t)
        {
            Assert.IsTrue(((IObjectRecorderModule)_module).StartRecordingObject(t, false, _ctx));
        }

        private void RunFrame()
        {
            var frameInfo = new FrameInfo((ulong)_frame, _frame++);
            var module = (IFrameDataRecorderModule)_module;
            _ctx.StopRecordingDestroyedObjectsIfRequested();
            module.BeforeEnqueueFrameData(frameInfo, _ctx);
            module.EnqueueFrameData(frameInfo, _ctx);
            module.AfterEnqueueFrameData(frameInfo, _ctx);
            module.SerializeFrameData(frameInfo, new FrameDataWriter(_out));
        }
    }
}
