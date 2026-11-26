using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tests.HooksFixture
{
    /// <summary>
    /// Calls hooked Unity APIs from a runtime assembly that does not reference the recorder, in the C# shapes the
    /// hooks post-processor must handle (lambdas, local functions, iterators, async state machines, generics, nested
    /// types). Each call creates exactly one GameObject.
    /// </summary>
    public static class HooksFixture
    {
        public const int CreatedGameObjectsCount = 9;

        public static List<GameObject> CreateGameObjects()
        {
            var created = new List<GameObject> { new GameObject("ctor"), GameObject.CreatePrimitive(PrimitiveType.Cube) };

            var original = created[0];
            created.Add(Object.Instantiate(original));
            created.Add(InstantiateGeneric(original));

            Func<GameObject> lambda = () => new GameObject("lambda");
            created.Add(lambda());
            created.Add(Local());
            GameObject Local() => new GameObject("local");

            var iterator = Iterator();
            while (iterator.MoveNext())
                created.Add((GameObject)iterator.Current);

            created.Add(CreateAsync().GetAwaiter().GetResult());
            created.Add(new Nested().Create());
            return created;
        }

        public static void AddComponents(GameObject go)
        {
            go.AddComponent<BoxCollider>();
            go.AddComponent(typeof(SphereCollider));
        }

        private static readonly MethodInfo SetActive = typeof(GameObject).GetMethod(nameof(GameObject.SetActive));

        public static void SetActiveThroughCreateDelegate(GameObject go, bool value) =>
            ((Action<GameObject, bool>)SetActive.CreateDelegate(typeof(Action<GameObject, bool>)))(go, value);

        public static void SetActiveThroughBoundDelegate(GameObject go, bool value) =>
            ((Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>), go, SetActive))(value);

        public static void SetActiveThroughInvoke(GameObject go, bool value) => SetActive.Invoke(go, new object[] { value });

        public static void SetActiveThroughExpression(GameObject go, bool value)
        {
            var target = Expression.Parameter(typeof(GameObject));
            var argument = Expression.Parameter(typeof(bool));
            Expression.Lambda<Action<GameObject, bool>>(Expression.Call(target, SetActive, argument), target, argument)
                .Compile()(go, value);
        }

        // Reflection calls are redirected to hooks: methods without a runtime handle must still be invoked.
        public static object InvokeDynamicMethod()
        {
            var method = new System.Reflection.Emit.DynamicMethod("Answer", typeof(int), Type.EmptyTypes);
            var il = method.GetILGenerator();
            il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, 42);
            il.Emit(System.Reflection.Emit.OpCodes.Ret);
            return method.Invoke(null, null);
        }

        private static T InstantiateGeneric<T>(T original) where T : Object => Object.Instantiate(original);

        private static IEnumerator Iterator()
        {
            yield return new GameObject("iterator");
        }

        private static async Task<GameObject> CreateAsync()
        {
            await Task.CompletedTask;
            return new GameObject("async");
        }

        private sealed class Nested
        {
            public GameObject Create() => new GameObject("nested");
        }
    }
}
