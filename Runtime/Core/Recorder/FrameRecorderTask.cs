using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;

namespace PLUME.Core.Recorder
{
    // TODO: convert to IUniTaskSource
    public class FrameRecorderTask
    {
        private static readonly ObjectPool<FrameRecorderTask> Pool = new(() => new FrameRecorderTask());
        
        public int Frame { get; private set; }
        public UniTask Task { get; private set; }

        private FrameRecorderTask()
        {
        }

        public static FrameRecorderTask Get(int frame, UniTask task)
        {
            var frameRecorderTask = Pool.Get();
            frameRecorderTask.Frame = frame;
            frameRecorderTask.Task = task;
            return frameRecorderTask;
        }

        public static void Release(FrameRecorderTask frameRecorderTask)
        {
            Pool.Release(frameRecorderTask);
        }
    }

    public class FrameRecorderTaskComparer : IEqualityComparer<FrameRecorderTask>
    {
        public static readonly FrameRecorderTaskComparer Instance = new();

        public bool Equals(FrameRecorderTask x, FrameRecorderTask y)
        {
            return x.Frame == y.Frame;
        }

        public int GetHashCode(FrameRecorderTask obj)
        {
            return obj.Frame;
        }
    }
}