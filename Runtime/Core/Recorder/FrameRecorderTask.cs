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

        public bool Equals(FrameRecorderTask other)
        {
            return Frame == other.Frame;
        }

        public override bool Equals(object obj)
        {
            return obj is FrameRecorderTask other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Frame;
        }
    }

    public class FrameRecorderModuleTaskComparer : IEqualityComparer<FrameRecorderTask>
    {
        public static readonly FrameRecorderModuleTaskComparer Instance = new();

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