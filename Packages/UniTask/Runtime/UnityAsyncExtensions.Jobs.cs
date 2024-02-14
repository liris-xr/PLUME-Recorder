#if ENABLE_MANAGED_JOBS
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Threading;
using Unity.Jobs;

namespace Cysharp.Threading.Tasks
{
    public static partial class UnityAsyncExtensions
    {
        
        public static UniTask WaitAsync(this JobHandle jobHandle, PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellationToken = default)
        {
            return new UniTask(JobHandlePromise.Create(jobHandle, timing, cancellationToken, out var token), token);
        }

        public sealed class JobHandlePromise : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<JobHandlePromise>
        {
            static TaskPool<JobHandlePromise> pool;
            JobHandlePromise nextNode;
            public ref JobHandlePromise NextNode => ref nextNode;

            static JobHandlePromise()
            {
                TaskPool.RegisterSizeGetter(typeof(JobHandlePromise), () => pool.Size);
            }

            JobHandle jobHandle;
            CancellationToken cancellationToken;

            UniTaskCompletionSourceCore<AsyncUnit> core;

            public static JobHandlePromise Create(JobHandle jobHandle, PlayerLoopTiming timing,
                CancellationToken cancellationToken, out short token)
            {
                if (!pool.TryPop(out var result))
                {
                    result = new JobHandlePromise();
                }

                result.jobHandle = jobHandle;
                result.cancellationToken = cancellationToken;

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public void GetResult(short token)
            {
                try
                {
                    core.GetResult(token);
                }
                finally
                {
                    TryReturn();
                }
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                jobHandle = default;
                cancellationToken = default;
                return pool.TryPush(this);
            }

            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    jobHandle.Complete();
                    return false;
                }

                if (jobHandle.IsCompleted | PlayerLoopHelper.IsEditorApplicationQuitting)
                {
                    jobHandle.Complete();
                    core.TrySetResult(AsyncUnit.Default);
                    return false;
                }

                return true;
            }
        }
    }
}

#endif