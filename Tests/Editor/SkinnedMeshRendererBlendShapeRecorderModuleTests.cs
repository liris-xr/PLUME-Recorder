using System;
using NUnit.Framework;
using PLUME.Base.Module.Unity.Renderer.SkinnedMeshRendererModule;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Settings;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Tests.Editor
{
    /// <summary>
    /// Exercises <see cref="SkinnedMeshRendererBlendShapeRecorderModule"/> in isolation, driving its per-frame
    /// collection directly through the module interfaces. Verifies that it polls current weights (regardless of the
    /// writer, which is the whole point of switching away from the SetBlendShapeWeight hook), honours the weight
    /// threshold, and — critically — allocates no managed memory in steady state.
    /// </summary>
    [TestFixture]
    public class SkinnedMeshRendererBlendShapeRecorderModuleTests
    {
        private const int BlendShapeCount = 8;

        private GameObject _go;
        private Mesh _mesh;
        private SkinnedMeshRenderer _smr;

        private SkinnedMeshRendererBlendShapeRecorderModule _module;
        private RecorderContext _ctx;
        private IComponentSafeRef<SkinnedMeshRenderer> _smrSafeRef;
        private NativeList<byte> _out;

        [SetUp]
        public void SetUp()
        {
            _mesh = new Mesh
            {
                vertices = new[] { Vector3.zero, Vector3.right, Vector3.up },
                triangles = new[] { 0, 1, 2 }
            };
            var delta = new Vector3[3];
            for (var s = 0; s < BlendShapeCount; s++)
            {
                delta[0] = new Vector3(0, s + 1, 0);
                _mesh.AddBlendShapeFrame("shape" + s, 100f, delta, null, null);
            }

            _go = new GameObject("smr_blendshape_test");
            _smr = _go.AddComponent<SkinnedMeshRenderer>();
            _smr.sharedMesh = _mesh;

            _module = new SkinnedMeshRendererBlendShapeRecorderModule();
            var modules = new List<IRecorderModule> { _module }.AsReadOnly();
            _ctx = new RecorderContext(modules, new SafeRefProvider(), new FileSettingsProvider());

            ((IRecorderModule)_module).Create(_ctx);
            _ctx.Status = RecorderStatus.Recording;

            _smrSafeRef = _ctx.SafeRefProvider.GetOrCreateComponentSafeRef(_smr);
            Assume.That(_smrSafeRef.IsNull, Is.False, "Safe ref could not be resolved (scene GUID missing?).");

            ((IObjectRecorderModule)_module).StartRecordingObject(_smrSafeRef, true, _ctx);

            _out = new NativeList<byte>(Allocator.Persistent);
        }

        [TearDown]
        public void TearDown()
        {
            if (_ctx != null)
                ((IRecorderModule)_module).StopRecording(_ctx);
            if (_out.IsCreated)
                _out.Dispose();
            if (_go != null)
                UnityEngine.Object.DestroyImmediate(_go);
            if (_mesh != null)
                UnityEngine.Object.DestroyImmediate(_mesh);
        }

        /// <summary>Runs one collect+serialize cycle and returns the number of bytes the module emitted this frame.</summary>
        private int RunFrame(int frameNumber)
        {
            var fi = new FrameInfo((ulong)frameNumber, frameNumber);
            var fdm = (IFrameDataRecorderModule)_module;

            fdm.BeforeEnqueueFrameData(fi, _ctx);
            fdm.EnqueueFrameData(fi, _ctx);
            fdm.AfterEnqueueFrameData(fi, _ctx);

            var before = _out.Length;
            fdm.SerializeFrameData(fi, new FrameDataWriter(_out));
            return _out.Length - before;
        }

        [Test]
        public void FirstFrame_EmitsInitialWeights()
        {
            Assert.That(RunFrame(0), Is.GreaterThan(0), "Initial weights should be recorded on the first frame.");
        }

        [Test]
        public void UnchangedWeights_EmitNothing()
        {
            RunFrame(0); // initial
            Assert.That(RunFrame(1), Is.Zero, "A frame with no weight change should emit nothing.");
            Assert.That(RunFrame(2), Is.Zero);
        }

        [Test]
        public void ChangeAboveThreshold_Emits_BelowThreshold_DoesNot()
        {
            RunFrame(0); // initial, baseline = 0

            // Below threshold (default 0.01): no emit.
            _smr.SetBlendShapeWeight(0, 0.005f);
            Assert.That(RunFrame(1), Is.Zero, "Sub-threshold change must not be recorded.");

            // Above threshold: emit.
            _smr.SetBlendShapeWeight(0, 5f);
            Assert.That(RunFrame(2), Is.GreaterThan(0), "Supra-threshold change must be recorded.");

            // Settle: no further change -> no emit.
            Assert.That(RunFrame(3), Is.Zero);
        }

        [Test]
        public void SubThresholdDrift_AccumulatesAgainstBaseline_AndEmitsOnCrossing()
        {
            RunFrame(0); // initial, baseline = 0

            // Each step is below the threshold relative to the previous frame, but the baseline stays at the last
            // recorded value (0), so the drift accumulates and must be recorded once the total crosses the threshold.
            _smr.SetBlendShapeWeight(0, 0.006f);
            Assert.That(RunFrame(1), Is.Zero, "0.006 is within the threshold of the baseline; nothing to record yet.");

            _smr.SetBlendShapeWeight(0, 0.012f);
            Assert.That(RunFrame(2), Is.GreaterThan(0), "Accumulated drift past the threshold must be recorded.");
        }

        [Test]
        public void SteadyState_ChangingEveryFrame_AllocatesNoManagedMemory()
        {
            // Warm up: first frame allocates the reused float[] buffer and the frame-data dictionary capacity.
            for (var f = 0; f < 32; f++)
            {
                _smr.SetBlendShapeWeight(0, f % 90);
                RunFrame(f);
            }

            GC.Collect();
            var before = GC.GetAllocatedBytesForCurrentThread();

            const int frames = 512;
            for (var f = 0; f < frames; f++)
            {
                // Change past threshold every frame to force the full changed path (native buffers + serialization).
                _smr.SetBlendShapeWeight(0, f % 90);
                RunFrame(1000 + f);
            }

            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero,
                $"Steady-state blend shape recording allocated {allocated} managed bytes over {frames} frames.");
        }

        [Test]
        public void SteadyState_Idle_AllocatesNoManagedMemory()
        {
            for (var f = 0; f < 32; f++)
                RunFrame(f);

            GC.Collect();
            var before = GC.GetAllocatedBytesForCurrentThread();

            const int frames = 512;
            for (var f = 0; f < frames; f++)
                RunFrame(1000 + f);

            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero,
                $"Idle blend shape recording allocated {allocated} managed bytes over {frames} frames.");
        }
    }
}
