using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PLUME.Core.Hooks;
using UnityEngine.Scripting;

namespace PLUME.Base.Hooks
{
    /// <summary>
    /// Calls built through reflection (delegates, MethodBase.Invoke, expression trees) have no call site for
    /// the hooks post-processor to rewrite, e.g. Visual Scripting nodes. These hooks redirect the reflection APIs
    /// themselves: when they target a hooked method, the matching hook is used instead. Calls to any other method are
    /// left untouched. A redirected delegate or expression reports the hook as its method (e.g. SetActiveAndNotify
    /// instead of SetActive): an assembly relying on that identity can be excluded through the hooks blacklist.
    /// </summary>
    [Preserve]
    public class ReflectionHooks : IRegisterHooksCallback
    {
        private static Dictionary<IntPtr, MethodInfo> _hookByTargetHandle;

        public void RegisterHooks(HooksRegistry hooksRegistry)
        {
            var self = typeof(ReflectionHooks);

            hooksRegistry.RegisterHook(self.GetMethod(nameof(CreateDelegateAndNotify), new[] { typeof(MethodInfo), typeof(Type) }),
                typeof(MethodInfo).GetMethod(nameof(MethodInfo.CreateDelegate), new[] { typeof(Type) }));
            hooksRegistry.RegisterHook(self.GetMethod(nameof(CreateDelegateAndNotify), new[] { typeof(MethodInfo), typeof(Type), typeof(object) }),
                typeof(MethodInfo).GetMethod(nameof(MethodInfo.CreateDelegate), new[] { typeof(Type), typeof(object) }));
            hooksRegistry.RegisterHook(self.GetMethod(nameof(DelegateCreateDelegateAndNotify), new[] { typeof(Type), typeof(object), typeof(MethodInfo) }),
                typeof(Delegate).GetMethod(nameof(Delegate.CreateDelegate), new[] { typeof(Type), typeof(object), typeof(MethodInfo) }));
            hooksRegistry.RegisterHook(self.GetMethod(nameof(DelegateCreateDelegateAndNotify), new[] { typeof(Type), typeof(MethodInfo) }),
                typeof(Delegate).GetMethod(nameof(Delegate.CreateDelegate), new[] { typeof(Type), typeof(MethodInfo) }));
            hooksRegistry.RegisterHook(self.GetMethod(nameof(InvokeAndNotify)),
                typeof(MethodBase).GetMethod(nameof(MethodBase.Invoke), new[] { typeof(object), typeof(object[]) }));
            hooksRegistry.RegisterHook(self.GetMethod(nameof(ExpressionCallAndNotify), new[] { typeof(Expression), typeof(MethodInfo), typeof(Expression[]) }),
                typeof(Expression).GetMethod(nameof(Expression.Call), new[] { typeof(Expression), typeof(MethodInfo), typeof(Expression[]) }));
            hooksRegistry.RegisterHook(self.GetMethod(nameof(ExpressionCallAndNotify), new[] { typeof(Expression), typeof(MethodInfo), typeof(IEnumerable<Expression>) }),
                typeof(Expression).GetMethod(nameof(Expression.Call), new[] { typeof(Expression), typeof(MethodInfo), typeof(IEnumerable<Expression>) }));
        }

        public static Delegate CreateDelegateAndNotify(MethodInfo method, Type delegateType)
        {
            return TryGetHook(method, out var hook) && TryCreate(() => hook.CreateDelegate(delegateType), out var redirected)
                ? redirected
                : method.CreateDelegate(delegateType);
        }

        public static Delegate CreateDelegateAndNotify(MethodInfo method, Type delegateType, object target)
        {
            // A static hook closed over its first argument behaves like the instance method bound to target.
            return TryGetHook(method, out var hook) && !method.IsStatic &&
                   TryCreate(() => Delegate.CreateDelegate(delegateType, target, hook), out var redirected)
                ? redirected
                : method.CreateDelegate(delegateType, target);
        }

        public static Delegate DelegateCreateDelegateAndNotify(Type delegateType, object target, MethodInfo method)
        {
            return TryGetHook(method, out var hook) && !method.IsStatic &&
                   TryCreate(() => Delegate.CreateDelegate(delegateType, target, hook), out var redirected)
                ? redirected
                : Delegate.CreateDelegate(delegateType, target, method);
        }

        public static Delegate DelegateCreateDelegateAndNotify(Type delegateType, MethodInfo method)
        {
            return TryGetHook(method, out var hook) && TryCreate(() => Delegate.CreateDelegate(delegateType, hook), out var redirected)
                ? redirected
                : Delegate.CreateDelegate(delegateType, method);
        }

        public static object InvokeAndNotify(MethodBase method, object obj, object[] parameters)
        {
            if (method is not MethodInfo methodInfo || !TryGetHook(methodInfo, out var hook))
                return method.Invoke(obj, parameters);

            if (methodInfo.IsStatic)
                return hook.Invoke(null, parameters);

            var hookParameters = new object[(parameters?.Length ?? 0) + 1];
            hookParameters[0] = obj;
            parameters?.CopyTo(hookParameters, 1);
            return hook.Invoke(null, hookParameters);
        }

        public static MethodCallExpression ExpressionCallAndNotify(Expression instance, MethodInfo method, params Expression[] arguments)
        {
            return ExpressionCallAndNotify(instance, method, (IEnumerable<Expression>)arguments);
        }

        public static MethodCallExpression ExpressionCallAndNotify(Expression instance, MethodInfo method, IEnumerable<Expression> arguments)
        {
            if (instance == null || !TryGetHook(method, out var hook))
                return Expression.Call(instance, method, arguments);

            return Expression.Call(hook, new[] { instance }.Concat(arguments ?? Enumerable.Empty<Expression>()));
        }

        private static bool TryGetHook(MethodInfo method, out MethodInfo hook)
        {
            hook = null;
            if (method == null)
                return false;

            var definition = method.IsGenericMethod ? method.GetGenericMethodDefinition() : method;
            if (!HookByTargetHandle.TryGetValue(definition.MethodHandle.Value, out hook))
                return false;

            if (method.IsGenericMethod)
                hook = hook.MakeGenericMethod(method.GetGenericArguments());
            return true;
        }

        private static bool TryCreate(Func<Delegate> create, out Delegate result)
        {
            try
            {
                result = create();
                return true;
            }
            catch (ArgumentException)
            {
                result = null;
                return false;
            }
        }

        private static Dictionary<IntPtr, MethodInfo> HookByTargetHandle
        {
            get
            {
                if (_hookByTargetHandle != null)
                    return _hookByTargetHandle;

                var registry = new HooksRegistry();
                registry.RegisterHooksInternal();
                var map = new Dictionary<IntPtr, MethodInfo>();
                foreach (var hook in registry.Hooks)
                {
                    // Only redirect Unity APIs: redirecting the reflection APIs to themselves would recurse.
                    if (hook.HookMethod.DeclaringType == typeof(ReflectionHooks) || hook.TargetMethod is not MethodInfo)
                        continue;
                    map[hook.TargetMethod.MethodHandle.Value] = hook.HookMethod;
                }

                return _hookByTargetHandle = map;
            }
        }
    }
}
