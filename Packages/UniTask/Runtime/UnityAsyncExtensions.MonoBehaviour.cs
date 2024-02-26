using System;
using System.Threading;
using UnityEngine;

namespace Cysharp.Threading.Tasks
{
    public static partial class UnityAsyncExtensions
    {
        public static UniTask StartAsyncCoroutine(this MonoBehaviour monoBehaviour,
            Func<CancellationToken, UniTask> asyncCoroutine)
        {
            var token = monoBehaviour.GetCancellationTokenOnDestroy();
            return asyncCoroutine(token);
        }
    }
}