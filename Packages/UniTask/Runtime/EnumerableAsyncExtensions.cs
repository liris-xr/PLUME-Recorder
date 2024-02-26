#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cysharp.Threading.Tasks
{
    public static class EnumerableAsyncExtensions
    {
        // overload resolver - .Select(async x => { }) : IEnumerable<UniTask<T>>

        public static IEnumerable<UniTask> Select<T>(this IEnumerable<T> source, Func<T, UniTask> selector)
        {
            return Enumerable.Select(source, selector);
        }

        public static IEnumerable<UniTask<TR>> Select<T, TR>(this IEnumerable<T> source, Func<T, UniTask<TR>> selector)
        {
            return Enumerable.Select(source, selector);
        }

        public static IEnumerable<UniTask> Select<T>(this IEnumerable<T> source, Func<T, int, UniTask> selector)
        {
            return Enumerable.Select(source, selector);
        }

        public static IEnumerable<UniTask<TR>> Select<T, TR>(this IEnumerable<T> source, Func<T, int, UniTask<TR>> selector)
        {
            return Enumerable.Select(source, selector);
        }
    }
}


