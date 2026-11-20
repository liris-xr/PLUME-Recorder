using System.Collections.Generic;
using NUnit.Framework;
using PLUME.Base.Hooks;
using PLUME.Base.Module.Unity.Events;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Core.Settings;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Tests.Editor
{
    /// <summary>
    /// Inspector listeners are invoked by Unity through reflection, out of reach of the hooks: the module must raise
    /// the hook notification of the change they made, and stop doing so once the component is no longer recorded.
    /// </summary>
    [TestFixture]
    public class UnityEventRecorderModuleTests
    {
        private readonly List<Object> _created = new();

        private UnityEventRecorderModule _module;
        private RecorderContext _ctx;
        private GameObject _target;
        private int _notified;

        [SetUp]
        public void SetUp()
        {
            _module = new UnityEventRecorderModule();
            var modules = new List<IRecorderModule> { _module }.AsReadOnly();
            _ctx = new RecorderContext(modules, new SafeRefProvider(), new FileSettingsProvider());
            ((IRecorderModule)_module).Create(_ctx);
            _ctx.Status = RecorderStatus.Recording;

            _target = new GameObject("target");
            _created.Add(_target);
            _notified = 0;
            GameObjectHooks.OnActiveChanged += OnActiveChanged;
        }

        [TearDown]
        public void TearDown()
        {
            GameObjectHooks.OnActiveChanged -= OnActiveChanged;
            ((IRecorderModule)_module).StopRecording(_ctx);
            _ctx.Status = RecorderStatus.Stopped;
            foreach (var obj in _created)
                Object.DestroyImmediate(obj);
            _created.Clear();
        }

        [Test]
        public void InspectorListener_OnParameterlessEvent_NotifiesTheChange()
        {
            var button = Create<Button>();
            UnityEventTools.AddBoolPersistentListener(button.onClick, _target.SetActive, false);
            EnableInEditMode(button.onClick);
            StartRecording(button);

            button.onClick.Invoke();

            Assert.IsFalse(_target.activeSelf);
            Assert.AreEqual(1, _notified);
        }

        [Test]
        public void InspectorListener_OnGenericEvent_NotifiesTheChange()
        {
            var toggle = Create<Toggle>();
            UnityEventTools.AddPersistentListener(toggle.onValueChanged, _target.SetActive);
            EnableInEditMode(toggle.onValueChanged);
            StartRecording(toggle);

            toggle.onValueChanged.Invoke(false);

            Assert.IsFalse(_target.activeSelf);
            Assert.AreEqual(1, _notified);
        }

        [Test]
        public void InspectorListener_AfterRecordingStopped_IsNotNotified()
        {
            var button = Create<Button>();
            UnityEventTools.AddBoolPersistentListener(button.onClick, _target.SetActive, false);
            EnableInEditMode(button.onClick);
            StartRecording(button);
            ((IRecorderModule)_module).StopRecording(_ctx);

            button.onClick.Invoke();

            Assert.IsFalse(_target.activeSelf);
            Assert.AreEqual(0, _notified);
        }

        [Test]
        public void Component_DestroyedWithoutHooks_StopsBeingRecorded()
        {
            var button = Create<Button>();
            var safeRef = _ctx.SafeRefProvider.GetOrCreateComponentSafeRef<MonoBehaviour>(button);
            StartRecording(button);

            // The hook handlers ignore what happens while not recording, as for code the hooks do not see.
            _ctx.Status = RecorderStatus.Stopped;
            Object.DestroyImmediate(button);
            _ctx.Status = RecorderStatus.Recording;

            var frameInfo = new FrameInfo(0, 0);
            var module = (IFrameDataRecorderModule)_module;
            module.BeforeEnqueueFrameData(frameInfo, _ctx);
            _ctx.StopRecordingDestroyedObjectsIfRequested();
            module.EnqueueFrameData(frameInfo, _ctx);
            module.AfterEnqueueFrameData(frameInfo, _ctx);

            Assert.IsFalse(_module.IsRecordingObject(safeRef));
        }

        private T Create<T>() where T : Component
        {
            var go = new GameObject(typeof(T).Name);
            _created.Add(go);
            return go.AddComponent<T>();
        }

        // Inspector listeners only run in play mode by default.
        private static void EnableInEditMode(UnityEventBase unityEvent)
        {
            unityEvent.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
        }

        private void StartRecording(MonoBehaviour component)
        {
            var safeRef = _ctx.SafeRefProvider.GetOrCreateComponentSafeRef(component);
            ((IObjectRecorderModule)_module).StartRecordingObject(safeRef, true, _ctx);
        }

        private void OnActiveChanged(GameObject go, bool active)
        {
            if (go == _target)
                _notified++;
        }
    }
}
