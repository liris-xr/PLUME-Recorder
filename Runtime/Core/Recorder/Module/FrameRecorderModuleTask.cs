using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;

namespace PLUME.Core.Recorder.Module
{
    // TODO: convert to IUniTaskSource
    public class FrameRecorderModuleTask
    {
        private static readonly ObjectPool<FrameRecorderModuleTask> Pool = new(() => new FrameRecorderModuleTask());
        
        public int Frame { get; private set; }
        public UniTask Task { get; private set; }

        private FrameRecorderModuleTask()
        {
        }

        public static FrameRecorderModuleTask Get(int frame, UniTask task)
        {
            var frameRecorderTask = Pool.Get();
            frameRecorderTask.Frame = frame;
            frameRecorderTask.Task = task;
            return frameRecorderTask;
        }

        public static void Release(FrameRecorderModuleTask frameRecorderModuleTask)
        {
            Pool.Release(frameRecorderModuleTask);
        }
    }

    public class FrameRecorderTaskComparer : IEqualityComparer<FrameRecorderModuleTask>
    {
        public static readonly FrameRecorderTaskComparer Instance = new();

        public bool Equals(FrameRecorderModuleTask x, FrameRecorderModuleTask y)
        {
            return x.Frame == y.Frame;
        }

        public int GetHashCode(FrameRecorderModuleTask obj)
        {
            return obj.Frame;
        }
    }
}