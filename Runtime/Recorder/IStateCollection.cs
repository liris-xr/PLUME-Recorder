using System;
using System.Collections;

namespace PLUME.Recorder
{
    public interface IStateCollection : ICollection
    {
        Type GetSampleType();

        void Clear();
    }
}