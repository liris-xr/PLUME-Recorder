using System.Collections.Generic;
using NUnit.Framework;
using PLUME.Base.Hooks;
using UnityEngine;
using Fixture = Tests.HooksFixture.HooksFixture;

namespace Tests.Editor
{
    /// <summary>
    /// End-to-end check of the hooks IL post-processor: calls compiled in another assembly must reach the hooks.
    /// </summary>
    [TestFixture]
    public class HooksInjectionTests
    {
        private readonly List<GameObject> _created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _created)
                Object.DestroyImmediate(go);
            _created.Clear();
        }

        [Test]
        public void CreatingGameObjects_InAnotherAssembly_NotifiesEveryCreation()
        {
            // Instantiate notifies the root once per Transform in the hierarchy, so compare objects, not calls.
            var notified = new HashSet<GameObject>();
            void OnCreated(GameObject go) => notified.Add(go);

            GameObjectHooks.OnCreated += OnCreated;
            try
            {
                _created.AddRange(Fixture.CreateGameObjects());
            }
            finally
            {
                GameObjectHooks.OnCreated -= OnCreated;
            }

            Assert.AreEqual(Fixture.CreatedGameObjectsCount, _created.Count);
            CollectionAssert.IsSubsetOf(_created, notified);
        }

        [Test]
        public void AddingComponents_InAnotherAssembly_NotifiesGenericAndNonGenericCalls()
        {
            var go = new GameObject();
            _created.Add(go);
            var notified = 0;
            void OnComponentAdded(GameObject target, Component component) => notified++;

            GameObjectHooks.OnComponentAdded += OnComponentAdded;
            try
            {
                Fixture.AddComponents(go);
            }
            finally
            {
                GameObjectHooks.OnComponentAdded -= OnComponentAdded;
            }

            Assert.AreEqual(2, notified);
        }
    }
}
