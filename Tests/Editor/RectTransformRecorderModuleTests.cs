using System.Collections.Generic;
using NUnit.Framework;
using PLUME.Base.Module.Unity.UI.RectTransform;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Settings;
using Unity.Collections;
using UnityEngine;

namespace Tests.Editor
{
    /// <summary>
    /// A RectTransform's layout (anchors, pivot, size, anchored position) must be recorded when it changes, not only
    /// when it is created.
    /// </summary>
    [TestFixture]
    public class RectTransformRecorderModuleTests
    {
        private RectTransformRecorderModule _module;
        private RecorderContext _ctx;
        private NativeList<byte> _out;
        private GameObject _go;
        private RectTransform _rectTransform;
        private int _frame;

        [SetUp]
        public void SetUp()
        {
            _module = new RectTransformRecorderModule();
            var modules = new List<IRecorderModule> { _module }.AsReadOnly();
            _ctx = new RecorderContext(modules, new SafeRefProvider(), new FileSettingsProvider());
            ((IRecorderModule)_module).Create(_ctx);
            ((IRecorderModule)_module).StartRecording(_ctx);
            _ctx.Status = RecorderStatus.Recording;
            _out = new NativeList<byte>(Allocator.Persistent);
            _frame = 0;

            _go = new GameObject("rect", typeof(RectTransform));
            _rectTransform = (RectTransform)_go.transform;
            var safeRef = _ctx.SafeRefProvider.GetOrCreateComponentSafeRef(_rectTransform);
            ((IObjectRecorderModule)_module).StartRecordingObject(safeRef, true, _ctx);
        }

        [TearDown]
        public void TearDown()
        {
            ((IRecorderModule)_module).StopRecording(_ctx);
            _ctx.Status = RecorderStatus.Stopped;
            _out.Dispose();
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void UnchangedLayout_EmitsNothing()
        {
            Assert.That(RunFrame(), Is.GreaterThan(0), "Creation should be recorded.");
            Assert.That(RunFrame(), Is.Zero);
        }

        [Test]
        public void ChangedLayout_IsRecorded()
        {
            RunFrame();

            _rectTransform.sizeDelta += Vector2.one;
            Assert.That(RunFrame(), Is.GreaterThan(0), "Size change should be recorded.");

            _rectTransform.anchorMin = new Vector2(0.25f, 0.25f);
            Assert.That(RunFrame(), Is.GreaterThan(0), "Anchor change should be recorded.");

            Assert.That(RunFrame(), Is.Zero);
        }

        private int RunFrame()
        {
            var frameInfo = new FrameInfo((ulong)_frame, _frame++);
            var module = (IFrameDataRecorderModule)_module;
            module.BeforeEnqueueFrameData(frameInfo, _ctx);
            module.EnqueueFrameData(frameInfo, _ctx);
            module.AfterEnqueueFrameData(frameInfo, _ctx);

            var before = _out.Length;
            module.SerializeFrameData(frameInfo, new FrameDataWriter(_out));
            return _out.Length - before;
        }
    }
}
