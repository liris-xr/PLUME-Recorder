#pragma warning disable CS1591
#pragma warning disable CS0108

#if (UNITASK_NETCORE && !NETSTANDARD2_0) || UNITY_2022_3_OR_NEWER
#define SUPPORT_VALUETASK
#endif

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;

namespace Cysharp.Threading.Tasks
{
    public enum UniTaskStatus
    {
        /// <summary>The operation has not yet completed.</summary>
        Pending = 0,

        /// <summary>The operation completed successfully.</summary>
        Succeeded = 1,

        /// <summary>The operation completed with an error.</summary>
        Faulted = 2,

        /// <summary>The operation completed due to cancellation.</summary>
        Canceled = 3
    }

    // similar as IValueTaskSource
    public interface IUniTaskSource
#if SUPPORT_VALUETASK
        : IValueTaskSource
#endif
    {
        UniTaskStatus GetStatus(short token);
        void OnCompleted(Action<object> continuation, object state, short token);
        void GetResult(short token);

        UniTaskStatus UnsafeGetStatus(); // only for debug use.

#if SUPPORT_VALUETASK

        ValueTaskSourceStatus IValueTaskSource.
            GetStatus(short token)
        {
            return (ValueTaskSourceStatus)(int)((IUniTaskSource)this).GetStatus(token);
        }

        void IValueTaskSource.GetResult(short token)
        {
            ((IUniTaskSource)this).GetResult(token);
        }

        void IValueTaskSource.OnCompleted(Action<object> continuation, object state,
            short token, ValueTaskSourceOnCompletedFlags flags)
        {
            // ignore flags, always none.
            ((IUniTaskSource)this).OnCompleted(continuation, state, token);
        }

#endif
    }

    public interface IUniTaskSource<out T> : IUniTaskSource
#if SUPPORT_VALUETASK
        , IValueTaskSource<T>
#endif
    {
        new T GetResult(short token);

#if SUPPORT_VALUETASK

        new public UniTaskStatus GetStatus(short token)
        {
            return ((IUniTaskSource)this).GetStatus(token);
        }

        new public void OnCompleted(Action<object> continuation, object state, short token)
        {
            ((IUniTaskSource)this).OnCompleted(continuation, state, token);
        }

        ValueTaskSourceStatus IValueTaskSource<T>.
            GetStatus(short token)
        {
            return (ValueTaskSourceStatus)(int)((IUniTaskSource)this).GetStatus(token);
        }

        T IValueTaskSource<T>.GetResult(short token)
        {
            return ((IUniTaskSource<T>)this).GetResult(token);
        }

        void IValueTaskSource<T>.OnCompleted(Action<object> continuation, object state,
            short token, ValueTaskSourceOnCompletedFlags flags)
        {
            // ignore flags, always none.
            ((IUniTaskSource)this).OnCompleted(continuation, state, token);
        }

#endif
    }

    public static class UniTaskStatusExtensions
    {
        /// <summary>status != Pending.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCompleted(this UniTaskStatus status)
        {
            return status != UniTaskStatus.Pending;
        }

        /// <summary>status == Succeeded.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCompletedSuccessfully(this UniTaskStatus status)
        {
            return status == UniTaskStatus.Succeeded;
        }

        /// <summary>status == Canceled.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCanceled(this UniTaskStatus status)
        {
            return status == UniTaskStatus.Canceled;
        }

        /// <summary>status == Faulted.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFaulted(this UniTaskStatus status)
        {
            return status == UniTaskStatus.Faulted;
        }
    }
}