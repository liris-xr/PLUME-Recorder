using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PLUME.Recorder.Module;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Object = UnityEngine.Object;

namespace PLUME.Recorder
{
    public class Recorder
    {
        private bool _isRecording;

        private readonly IRecorderModule[] _modules;

        private readonly FrameRecorder _frameRecorder;
        private readonly List<UniTask> _recordFrameTasks = new();

        public Recorder(IRecorderModule[] modules)
        {
            _modules = modules;

            var frameRecorderModules = _modules
                .Where(m => m is IUnityFrameRecorderModule)
                .Cast<IUnityFrameRecorderModule>().ToArray();
            var frameRecorderAsyncModules = _modules
                .Where(m => m is IUnityFrameRecorderModuleAsync)
                .Cast<IUnityFrameRecorderModuleAsync>().ToArray();
            _frameRecorder = new FrameRecorder(frameRecorderModules, frameRecorderAsyncModules);
        }

        public void Start()
        {
            Array.ForEach(_modules, m => m.Start());
            _isRecording = true;
        }

        public void Stop()
        {
            _isRecording = false;
            Array.ForEach(_modules, m => m.Stop());

            // Wait for all the frame recording tasks to finish.
            lock (_recordFrameTasks)
            {
                if(_recordFrameTasks.Count > 0)
                    Debug.Log("Waiting for " + _recordFrameTasks.Count + " frame recording tasks to finish");
                
                foreach (var task in _recordFrameTasks)
                {
                    task.AsTask().Wait();
                }

                _recordFrameTasks.Clear();
            }
        }

        public void Initialize()
        {
            InjectUpdateLoop();
        }

        private void InjectUpdateLoop()
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopUtils.AppendToPlayerLoopList(typeof(PlumeRecorderUpdateLoop), Update, ref loop,
                typeof(PostLateUpdate));
            PlayerLoop.SetPlayerLoop(loop);
        }

        internal void Update()
        {
            if (!_isRecording) return;

            var recordFrameTask = _frameRecorder.RecordFrameAsync(0L, Time.frameCount);
            var finalizeRecordFrameTask = recordFrameTask.ContinueWith(frameData =>
            {
                Debug.Log("Pushing frame data to recorder: Frame " + frameData.Frame + ", " +
                          frameData.Buffer.Data.Length + " bytes");
            });

            // Remove the task from the list when it's done.
            finalizeRecordFrameTask.ContinueWith(() =>
            {
                lock (_recordFrameTasks)
                {
                    _recordFrameTasks.Remove(finalizeRecordFrameTask);
                }
            });

            lock (_recordFrameTasks)
            {
                _recordFrameTasks.Add(finalizeRecordFrameTask);
            }
        }

        public bool TryStartRecordingObject<T>(ObjectSafeRef<T> objectSafeRef, bool markCreated)
            where T : Object
        {
            var started = false;

            foreach (var module in _modules)
            {
                if (module is IUnityObjectRecorderModule objectRecorderModule)
                {
                    started |= objectRecorderModule.TryStartRecordingObject(objectSafeRef, markCreated);
                }
            }

            return started;
        }

        public bool TryStopRecordingObject<T>(ObjectSafeRef<T> objectSafeRef) where T : Object
        {
            var stopped = false;

            foreach (var module in _modules)
            {
                if (module is IUnityObjectRecorderModule objectRecorderModule)
                {
                    stopped |= objectRecorderModule.TryStopRecordingObject(objectSafeRef);
                }
            }

            return stopped;
        }

        private struct PlumeRecorderUpdateLoop
        {
        }
    }
}