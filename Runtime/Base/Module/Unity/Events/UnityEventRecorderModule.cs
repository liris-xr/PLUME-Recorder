using System;
using System.Collections.Generic;
using System.Reflection;
using PLUME.Base.Hooks;
using PLUME.Core;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PLUME.Base.Module.Unity.Events
{
    /// <summary>
    /// Records the state changes made by UnityEvent listeners configured in the inspector (e.g. a button calling
    /// GameObject.SetActive). Unity invokes them by reflection from its own code, which no hook can rewrite, so they
    /// would otherwise be missing from the record. Runtime listeners run after the inspector ones: a listener added
    /// to each event raises, once the event has fired, the hook notification of every hooked method it called.
    /// </summary>
    [Preserve]
    public class UnityEventRecorderModule : ComponentRecorderModule<MonoBehaviour, UnityEventRecorderModule.NoFrameData>
    {
        private const BindingFlags EventFieldFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private delegate Action Subscriber(UnityEventBase unityEvent, Action onInvoked);

        // Per component type: its UnityEvent fields. Per event type: how to add a listener of the right signature.
        private static readonly Dictionary<Type, FieldInfo[]> EventFieldsByType = new();
        private static readonly Dictionary<Type, Subscriber> SubscribersByEventType = new();

        private readonly Dictionary<IComponentSafeRef<MonoBehaviour>, List<Action>> _unsubscribers = new();

        protected override void OnStartRecordingObject(IComponentSafeRef<MonoBehaviour> objSafeRef,
            RecorderContext ctx)
        {
            base.OnStartRecordingObject(objSafeRef, ctx);

            var component = objSafeRef.Component;
            List<Action> unsubscribers = null;

            foreach (var field in GetEventFields(component.GetType()))
            {
                if (field.GetValue(component) is not UnityEventBase unityEvent)
                    continue;

                var subscriber = GetSubscriber(unityEvent.GetType());
                if (subscriber == null)
                    continue;

                unsubscribers ??= new List<Action>();
                unsubscribers.Add(subscriber(unityEvent, () => OnEventInvoked(unityEvent, ctx)));
            }

            if (unsubscribers != null)
                _unsubscribers[objSafeRef] = unsubscribers;
        }

        protected override void OnStopRecordingObject(IComponentSafeRef<MonoBehaviour> objSafeRef,
            RecorderContext ctx)
        {
            base.OnStopRecordingObject(objSafeRef, ctx);

            if (!_unsubscribers.Remove(objSafeRef, out var unsubscribers))
                return;

            foreach (var unsubscribe in unsubscribers)
                unsubscribe();
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);

            foreach (var unsubscribers in _unsubscribers.Values)
                foreach (var unsubscribe in unsubscribers)
                    unsubscribe();

            _unsubscribers.Clear();
        }

        protected override NoFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            return null;
        }

        private void OnEventInvoked(UnityEventBase unityEvent, RecorderContext ctx)
        {
            if (!ctx.IsRecordingWith(this))
                return;

            // Called from the application's event invocation: never let a failure escape into it.
            try
            {
                for (var i = 0; i < unityEvent.GetPersistentEventCount(); i++)
                {
                    if (unityEvent.GetPersistentListenerState(i) == UnityEventCallState.Off)
                        continue;

                    var target = unityEvent.GetPersistentTarget(i);
                    if (target != null)
                        NotifyStateChange(target, unityEvent.GetPersistentMethodName(i));
                }
            }
            catch (Exception e)
            {
                ErrorReporter.Report($"{nameof(UnityEventRecorderModule)}.{nameof(OnEventInvoked)}",
                    "Failed to record the changes made by a UnityEvent. The application is not affected.", e);
            }
        }

        /// <summary>
        /// Raises the hook notification matching an inspector call, with the target's current state. Only the
        /// hooked methods an inspector can call are listed: instance methods taking at most one argument.
        /// </summary>
        private static void NotifyStateChange(Object target, string methodName)
        {
            switch (target)
            {
                case UnityEngine.GameObject go when methodName == nameof(UnityEngine.GameObject.SetActive):
                    GameObjectHooks.NotifyActiveChanged(go, go.activeSelf);
                    return;
                case UnityEngine.GameObject go when methodName == "set_tag":
                    GameObjectHooks.NotifyTagChanged(go, go.tag);
                    return;
                case UnityEngine.Transform t when methodName is nameof(UnityEngine.Transform.SetParent) or "set_parent":
                    TransformHooks.NotifyParentChanged(t, t.parent);
                    return;
                case UnityEngine.Transform t when methodName is nameof(UnityEngine.Transform.SetSiblingIndex)
                    or nameof(UnityEngine.Transform.SetAsFirstSibling) or nameof(UnityEngine.Transform.SetAsLastSibling):
                    TransformHooks.NotifySiblingIndexChanged(t, t.GetSiblingIndex());
                    return;
                case UnityEngine.SkinnedMeshRenderer skinnedMeshRenderer when methodName == "set_rootBone":
                    SkinnedMeshRendererHooks.NotifyRootBoneChanged(skinnedMeshRenderer, skinnedMeshRenderer.rootBone);
                    return;
                case UnityEngine.LineRenderer lineRenderer when methodName == "set_widthMultiplier":
                    LineRendererHooks.NotifyWidthMultiplierChanged(lineRenderer, lineRenderer.widthMultiplier);
                    return;
                case UnityEngine.Renderer renderer when methodName == "set_enabled":
                    RendererHooks.NotifyEnabledChanged(renderer, renderer.enabled);
                    return;
                // Reading material/mesh would instantiate a copy: the shared value is the one just assigned.
                case UnityEngine.Renderer renderer when methodName == "set_material":
                    RendererHooks.NotifyMaterialChanged(renderer, renderer.sharedMaterial);
                    return;
                case UnityEngine.Renderer renderer when methodName == "set_sharedMaterial":
                    RendererHooks.NotifySharedMaterialChanged(renderer, renderer.sharedMaterial);
                    return;
                case UnityEngine.MeshFilter meshFilter when methodName == "set_mesh":
                    MeshFilterHooks.NotifyMeshChanged(meshFilter, meshFilter.sharedMesh);
                    return;
                case UnityEngine.MeshFilter meshFilter when methodName == "set_sharedMesh":
                    MeshFilterHooks.NotifySharedMeshChanged(meshFilter, meshFilter.sharedMesh);
                    return;
                case Text text when methodName == "set_text":
                    TextHooks.NotifyTextChanged(text, text.text);
                    return;
                case not null when methodName == "set_name":
                    ObjectHooks.NotifyNameChanged(target, target.name);
                    return;
            }
        }

        private static FieldInfo[] GetEventFields(Type componentType)
        {
            if (EventFieldsByType.TryGetValue(componentType, out var fields))
                return fields;

            var eventFields = new List<FieldInfo>();
            for (var type = componentType; type != null && type != typeof(MonoBehaviour); type = type.BaseType)
            {
                foreach (var field in type.GetFields(EventFieldFlags))
                {
                    if (typeof(UnityEventBase).IsAssignableFrom(field.FieldType))
                        eventFields.Add(field);
                }
            }

            return EventFieldsByType[componentType] = eventFields.ToArray();
        }

        /// <summary>
        /// Listeners must match the event's signature (UnityAction, UnityAction&lt;T0&gt;...): a generic subscriber
        /// is built once per event type from its UnityEvent base class.
        /// </summary>
        private static Subscriber GetSubscriber(Type eventType)
        {
            if (SubscribersByEventType.TryGetValue(eventType, out var subscriber))
                return subscriber;

            // UnityEvent, UnityEvent<T0>... are the direct subclasses of UnityEventBase.
            var baseType = eventType;
            while (baseType != null && baseType.BaseType != typeof(UnityEventBase))
                baseType = baseType.BaseType;

            if (baseType?.Namespace == typeof(UnityEventBase).Namespace)
            {
                var arguments = baseType.IsGenericType ? baseType.GetGenericArguments() : Type.EmptyTypes;
                var method = typeof(UnityEventRecorderModule).GetMethod("Subscribe" + arguments.Length,
                    BindingFlags.NonPublic | BindingFlags.Static)!;
                if (arguments.Length > 0)
                    method = method.MakeGenericMethod(arguments);
                subscriber = (Subscriber)method.CreateDelegate(typeof(Subscriber));
            }

            return SubscribersByEventType[eventType] = subscriber;
        }

        private static Action Subscribe0(UnityEventBase unityEvent, Action onInvoked)
        {
            var e = (UnityEvent)unityEvent;
            UnityAction listener = () => onInvoked();
            e.AddListener(listener);
            return () => e.RemoveListener(listener);
        }

        private static Action Subscribe1<T0>(UnityEventBase unityEvent, Action onInvoked)
        {
            var e = (UnityEvent<T0>)unityEvent;
            UnityAction<T0> listener = _ => onInvoked();
            e.AddListener(listener);
            return () => e.RemoveListener(listener);
        }

        private static Action Subscribe2<T0, T1>(UnityEventBase unityEvent, Action onInvoked)
        {
            var e = (UnityEvent<T0, T1>)unityEvent;
            UnityAction<T0, T1> listener = (_, _) => onInvoked();
            e.AddListener(listener);
            return () => e.RemoveListener(listener);
        }

        private static Action Subscribe3<T0, T1, T2>(UnityEventBase unityEvent, Action onInvoked)
        {
            var e = (UnityEvent<T0, T1, T2>)unityEvent;
            UnityAction<T0, T1, T2> listener = (_, _, _) => onInvoked();
            e.AddListener(listener);
            return () => e.RemoveListener(listener);
        }

        private static Action Subscribe4<T0, T1, T2, T3>(UnityEventBase unityEvent, Action onInvoked)
        {
            var e = (UnityEvent<T0, T1, T2, T3>)unityEvent;
            UnityAction<T0, T1, T2, T3> listener = (_, _, _, _) => onInvoked();
            e.AddListener(listener);
            return () => e.RemoveListener(listener);
        }

        /// <summary>
        /// This module records through the hooks' notifications: it has no frame data of its own.
        /// </summary>
        public sealed class NoFrameData : IFrameData
        {
            public void Serialize(FrameDataWriter frameDataWriter)
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
