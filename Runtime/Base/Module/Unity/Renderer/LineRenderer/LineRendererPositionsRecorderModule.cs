using System;
using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using LineRendererSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.LineRenderer>;
using LineRendererPositionsUpdate = PLUME.Sample.ProtoBurst.Unity.LineRendererPositionsUpdate;

namespace PLUME.Base.Module.Unity.Renderer.LineRenderer
{
    /// <summary>
    /// Records line renderer positions by polling every recorded line once per frame on a dedicated,
    /// allocation-free stream (decoupled from <see cref="PLUME.Sample.Unity.LineRendererUpdate"/>).
    /// <para>
    /// Polling (rather than relying on the <c>LineRenderer.SetPosition(s)</c> hooks) is required because lines such
    /// as XR Interaction Toolkit interactor rays are rewritten every frame from package code, where the injected
    /// hooks never ran: those lines were recorded with no positions at all.
    /// </para>
    /// <para>
    /// A line only emits a <see cref="PLUME.Sample.Unity.LineRendererPositionsUpdate"/> when its position count
    /// changes or one of its positions moved by more than <see cref="PositionThreshold"/> on any axis since the last
    /// recorded positions. Positions are read into a reused native buffer and compared against a reused per-line
    /// array, so steady state does no managed allocation; native buffers are only allocated in frames where at least
    /// one line changed.
    /// </para>
    /// </summary>
    [Preserve]
    public class LineRendererPositionsRecorderModule :
        ComponentRecorderModule<UnityEngine.LineRenderer, LineRendererPositionsFrameData>
    {
        /// <summary>Per-axis change, in the line's space units (usually meters), below which a move is ignored.</summary>
        public const float PositionThreshold = 1e-4f;

        // Last recorded positions per line. Allocated when the line is first polled and only reallocated when its
        // position count changes, so a line moving every frame with a fixed count allocates nothing.
        private readonly Dictionary<LineRendererSafeRef, Vector3[]> _previousPositions = new();

        // Scratch buffer receiving LineRenderer.GetPositions, grown to the largest position count seen.
        private NativeArray<Vector3> _scratch;

        protected override void OnDestroy(RecorderContext ctx)
        {
            base.OnDestroy(ctx);

            if (_scratch.IsCreated)
                _scratch.Dispose();
        }

        protected override void OnObjectMarkedCreated(LineRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);

            // Force a full record in the creation frame, even if an instance id was reused.
            _previousPositions.Remove(objSafeRef);
        }

        protected override void OnStopRecordingObject(LineRendererSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnStopRecordingObject(objSafeRef, ctx);
            _previousPositions.Remove(objSafeRef);
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            _previousPositions.Clear();
        }

        protected override LineRendererPositionsFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var recorded = RecordedComponents;

            // Allocated lazily on the first changed line so that a frame in which no line moves does no allocation
            // at all. Left un-created otherwise; the frame data handles un-created lists in Serialize/Dispose.
            var updateSamples = default(NativeList<LineRendererPositionsUpdate>);
            var positions = default(NativeList<Vector3>);

            for (var r = 0; r < recorded.Count; r++)
            {
                var objSafeRef = recorded[r];
                var lineRenderer = objSafeRef.Component;

                if (lineRenderer == null)
                    continue;

                var count = lineRenderer.positionCount;
                EnsureScratchCapacity(count);

                if (count > 0)
                    lineRenderer.GetPositions(_scratch);

                var hasPrevious = _previousPositions.TryGetValue(objSafeRef, out var previous);
                var changed = !hasPrevious || previous.Length != count;

                // Compare against the last recorded positions without touching the baseline, so a slow drift still
                // accumulates past the threshold and is eventually recorded.
                if (!changed)
                {
                    for (var i = 0; i < count; i++)
                    {
                        var a = _scratch[i];
                        var b = previous[i];

                        if (Math.Abs(a.x - b.x) > PositionThreshold ||
                            Math.Abs(a.y - b.y) > PositionThreshold ||
                            Math.Abs(a.z - b.z) > PositionThreshold)
                        {
                            changed = true;
                            break;
                        }
                    }
                }

                if (!changed)
                    continue;

                if (!hasPrevious || previous.Length != count)
                {
                    previous = new Vector3[count];
                    _previousPositions[objSafeRef] = previous;
                }

                if (!positions.IsCreated)
                {
                    // First change this frame: allocate the shared buffers now (and only now).
                    updateSamples =
                        new NativeList<LineRendererPositionsUpdate>(recorded.Count - r, Allocator.Persistent);
                    positions = new NativeList<Vector3>(count, Allocator.Persistent);
                }

                // Record the current positions and advance the baseline to them.
                var offset = positions.Length;

                if (count > 0)
                {
                    var current = _scratch.GetSubArray(0, count);
                    positions.AddRange(current);
                    NativeArray<Vector3>.Copy(current, 0, previous, 0, count);
                }

                updateSamples.Add(new LineRendererPositionsUpdate
                {
                    Component = objSafeRef.Identifier,
                    PositionsOffset = offset,
                    PositionsCount = count
                });
            }

            return new LineRendererPositionsFrameData(updateSamples, positions);
        }

        private void EnsureScratchCapacity(int count)
        {
            if (_scratch.IsCreated && _scratch.Length >= count)
                return;

            // Grow geometrically so a line whose count creeps up does not reallocate every frame.
            var capacity = Math.Max(64, Math.Max(count, _scratch.IsCreated ? _scratch.Length * 2 : 0));

            if (_scratch.IsCreated)
                _scratch.Dispose();

            _scratch = new NativeArray<Vector3>(capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }
    }
}
