# Record animator-driven blend shapes via per-frame polling

**Date:** 2026-07-18
**Branch:** perf/blendshape-gc
**Status:** Approved design

## Problem

Blend shape weights driven by an `Animator` are not recorded. The current
`SkinnedMeshRendererBlendShapeRecorderModule` is **hook-driven**: it only marks a
renderer dirty when application code calls `SkinnedMeshRenderer.SetBlendShapeWeight`,
which is intercepted by the injected `SkinnedMeshRendererHooks.SetBlendShapeWeightAndNotify`
hook firing `OnBlendShapeWeightChanged`.

The animator writes blend shape weights internally through the playable-graph
property binding, bypassing the managed `SetBlendShapeWeight` setter entirely. The
hook therefore never fires, and animator-driven facial animation is silently dropped
from the record.

## Goal

Record blend shape weights driven by any source (animator, script, timeline) by
pulling them every frame, in a way that barely affects performance whether recording
is on or off:

- **Recording off:** zero per-frame cost (recorder modules do no work when not
  recording — already true, must stay true).
- **Recording on:** minimal cost — one native weight read per blend shape per
  recorded renderer per frame, an inline threshold compare, and no managed
  allocation in steady state.

## Root cause (confirmed)

`Runtime/Base/Module/Unity/Renderer/SkinnedMeshRendererModule/SkinnedMeshRendererBlendShapeRecorderModule.cs`
subscribes to `SkinnedMeshRendererHooks.OnBlendShapeWeightChanged` and only records
renderers present in its `_dirty` set. Animator-driven writes never pass through the
hooked setter, so those renderers are never added to `_dirty`.

## Approach: poll every frame with threshold diff

Model the module on `TransformRecorderModule`, which already pulls transform state
every frame and only emits an update when the change exceeds a configurable
threshold. Blend shape weights have no off-main-thread read API (unlike transforms,
which use `TransformAccessArray` + `IJobParallelForTransform`), so reads happen on
the main thread inside `CollectFrameData`. Read cost is negligible
(`GetBlendShapeWeight` is a native call of ~tens of nanoseconds; even 50 avatars ×
100 shapes ≈ a few thousand reads ≈ well under a millisecond).

### Module changes

`SkinnedMeshRendererBlendShapeRecorderModule`:

- **Remove** the `OnBlendShapeWeightChanged` subscription and the `_dirty`
  `HashSet`.
- **Add** per-renderer previous-weights storage: `Dictionary<SkinnedMeshRendererSafeRef, float[]>`.
  The `float[]` for a renderer is allocated once when it starts being recorded (and
  reallocated only if the mesh's `blendShapeCount` changes), then reused every frame.
  No per-frame managed allocation → preserves the branch's GC-free guarantee.
- **`CollectFrameData`** iterates `RecordedComponents` (every recorded renderer, not
  a dirty subset — the animator gives us no dirty signal):
  1. Skip renderers where `Component == null`, `sharedMesh == null`, or
     `blendShapeCount <= 0`.
  2. Let `count = sharedMesh.blendShapeCount`. Fetch/allocate the renderer's stored
     buffer; if its length != `count` (mesh swapped) or the renderer is newly
     recorded, treat as changed and (re)allocate.
  3. Read all `count` current weights via `GetBlendShapeWeight(i)`. Compare each
     against the stored previous with `abs(current - previous) > weightThreshold`.
  4. If any weight changed (or first frame / count changed): append the full current
     weight array to the frame's shared `NativeList<float>`, emit one
     `SkinnedMeshRendererBlendShapeUpdate { Component, WeightsOffset, WeightsCount }`,
     and copy current weights into the stored buffer.
  5. Unchanged renderers emit nothing.
- **`OnStartRecordingObject` / equivalent:** ensure a stored buffer exists (or lazily
  create on first `CollectFrameData`).
- **`OnStopRecordingObject`:** remove the renderer's entry from the dictionary.
- **`OnStopRecording`:** clear the dictionary.

The initial-frame full record (previously handled by adding to `_dirty` in
`OnObjectMarkedCreated`) is preserved naturally: a newly recorded renderer has no
stored previous buffer, so its first `CollectFrameData` counts as changed and emits
the full weights.

### Serialization / proto: unchanged

`SkinnedMeshRendererBlendShapeFrameData` and the `SkinnedMeshRendererBlendShapeUpdate`
sample (full packed weight array referenced by offset/count into a shared
`NativeList<float>`) are reused as-is. No proto or player changes.

### New setting

Add `SkinnedMeshRendererBlendShapeRecorderModuleSettings` (mirroring
`TransformRecorderModuleSettings` + its editor `SettingsProvider`), exposing:

- `weightThreshold` — minimum absolute weight change (any single blend shape) required
  to record an update for a renderer. Default `0.01`. Unity blend shape weights are
  conventionally 0–100, so `0.01` is a fine granularity while filtering float noise.
  Clamped to `>= 0` in `OnValidate`.

Read once in `OnCreate` like `TransformRecorderModule` reads its thresholds.

## Performance fallback (documented, not built now)

Primary implementation keeps the diff inline on the main thread with a managed reused
`float[]` per renderer. If profiling on real scenes shows the inline diff is a
bottleneck, escalate to a Burst diff:

- Store previous weights in a persistent `NativeList<float>` aligned to recorded
  renderers with swap-back index bookkeeping (as `TransformRecorderModule` does with
  `_alignedStates`).
- Read current weights into a native scratch buffer on the main thread (read is still
  main-thread-only), then run a `BurstCompile` job to diff against previous, produce
  the changed-renderer mask, and build the update samples.

This is a strictly larger change (native bookkeeping, swap-back on stop) with no
benefit unless the inline compare actually shows up in a profile — the reads
dominate and cannot be moved off the main thread either way. Kept as an option, not
in the initial scope.

## Testing

Minimal isolated scene first, then verify on the real scene:

1. **Temp scene:** one GameObject with a `SkinnedMeshRenderer` whose mesh has blend
   shapes, plus an `Animator` with a clip animating at least one blend shape weight
   over time. Play + record.
   - Confirm `SkinnedMeshRendererBlendShapeUpdate` samples appear in the stream while
     the clip animates.
   - Confirm no updates emit once the animated weight holds constant (threshold works).
   - Confirm the initial weights are recorded on the first recorded frame.
2. **Real scene:** `Assets/Scenes/Desktop/Trials/HF_Desk_BusStop.unity` — record a
   session, confirm animator-driven facial blend shapes are captured, and sanity-check
   no per-frame GC allocation via the profiler.

## Out of scope

- Sparse per-index weight deltas (proto sends the full array; a face rig changing one
  of N shapes still sends all N). Would require proto + player changes; not needed to
  fix the recording gap.
- Off-main-thread weight reads (no Unity API exists).
- The Burst-diff fallback above, unless profiling demands it.
