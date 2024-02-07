using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace PLUME.Recorder
{
    public class Recorder
    {
        private bool _isRecording;

        private readonly List<UniTask> _serializationTasks;
        private readonly UnityFrameRecorder _unityFrameRecorder;
        private readonly UnityFrameSerializer _unityFrameSerializer;

        public Recorder(UnityFrameRecorder unityFrameRecorder, UnityFrameSerializer unityFrameSerializer)
        {
            _serializationTasks = new List<UniTask>();
            _unityFrameRecorder = unityFrameRecorder;
            _unityFrameSerializer = unityFrameSerializer;
        }

        public void Start()
        {
            _unityFrameRecorder.Start();
            _isRecording = true;
        }

        public void Stop()
        {
            _isRecording = false;
            _unityFrameRecorder.Stop();
            lock (_serializationTasks) UniTask.WhenAll(_serializationTasks).AsTask().Wait();
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

        private void Update()
        {
            if (!_isRecording) return;

            // TODO: get timestamp from clock
            var frameData = _unityFrameRecorder.RecordFrame(42);
            var frameSerializeTask = _unityFrameSerializer.SerializeFrameAsync(frameData, null);

            frameSerializeTask.ContinueWith(() =>
            {
                lock (_serializationTasks) _serializationTasks.Remove(frameSerializeTask);
            });

            lock (_serializationTasks) _serializationTasks.Add(frameSerializeTask);
        }
    }
}