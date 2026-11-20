using System;
using System.Collections;
using System.Collections.Generic;
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
