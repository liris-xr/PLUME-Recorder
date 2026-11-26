using NUnit.Framework;
using PLUME.Core.Hooks;
using UnityEngine;

namespace Tests.Editor
{
    [TestFixture]
    public class HooksTests
    {
        [Test]
        public void EnsureHookIsCompatibleSimplePasses()
        {
            var targetMethod = typeof(HooksTests).GetMethod(nameof(GenericMethod1));
            var hookMethod = typeof(HooksTests).GetMethod(nameof(GenericMethodHook));
            Assert.DoesNotThrow(() => HooksRegistry.CheckHookMethodValidity(hookMethod, targetMethod));
        }

        [Test]
        public void EnsureHookIsCompatibleWithTypeConstraintsThrowsIncompatibleConstraints()
        {
            var targetMethod = typeof(HooksTests).GetMethod(nameof(GenericMethod2));
            var hookMethod = typeof(HooksTests).GetMethod(nameof(GenericMethodHook));
            Assert.Throws<InvalidHookMethodException>(() =>
                HooksRegistry.CheckHookMethodValidity(hookMethod, targetMethod));
        }

        [Test]
        public void RegisterHook_WithMissingTarget_SkipsItAndReportsWhy()
        {
            var registry = new HooksRegistry();
            var hookMethod = typeof(HooksTests).GetMethod(nameof(GenericMethodHook));

            // A target removed or renamed in another Unity version is found as null.
            Assert.DoesNotThrow(() => registry.RegisterHook(hookMethod, null));

            Assert.IsEmpty(registry.Hooks);
            Assert.AreEqual(1, registry.Errors.Count);
        }

        [Test]
        public void HookedReflectionCall_OnDynamicMethod_IsInvoked()
        {
            Assert.AreEqual(42, Tests.HooksFixture.HooksFixture.InvokeDynamicMethod());
        }

        public static T GenericMethod1<T>(T value) where T : struct
        {
            return value;
        }

        public static T GenericMethod2<T>(T value) where T : class
        {
            return value;
        }

        public static T GenericMethodHook<T>(T value) where T : struct
        {
            Debug.Log("Hello from the hook method!");
            return GenericMethod1(value);
        }
    }
}