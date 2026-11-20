using System.Runtime.CompilerServices;

// Hooked assemblies reference this assembly, which must therefore never reference PLUME.Recorder: an assembly that
// they reference is resolved by Mono before Unity reloads it, so a change to it would not be picked up until the
// editor restarts. PLUME.Recorder references this assembly instead.
[assembly: InternalsVisibleTo("PLUME.Recorder")]
[assembly: InternalsVisibleTo("PLUME.Recorder.Editor")]
[assembly: InternalsVisibleTo("PLUME.Recorder.Tests.Editor")]
[assembly: InternalsVisibleTo("Unity.PLUME.Recorder.CodeGen")]
