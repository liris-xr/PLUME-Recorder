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