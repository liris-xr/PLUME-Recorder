using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace PLUME.Core.Recorder
{
    public readonly struct FrameRecorderTask
    {
        public readonly int Frame;
        public readonly UniTask Task;

        public FrameRecorderTask(int frame, UniTask task)
        {
            Frame = frame;
            Task = task;
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