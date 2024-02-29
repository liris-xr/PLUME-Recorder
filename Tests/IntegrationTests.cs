using System.Collections;
using Cysharp.Threading.Tasks;
using PLUME.Core.Recorder;
using UnityEngine;
using UnityEngine.TestTools;

namespace PLUME.Tests
{
    public class IntegrationTests
    {
        [UnityTest]
        public IEnumerator IntegrationTest() => UniTask.ToCoroutine(async () =>
        {
            PlumeRecorder.Instantiate();
            var go1 = new GameObject("go1");
            var go2 = new GameObject("go2");

            PlumeRecorder.StartRecording("test");
            PlumeRecorder.StartRecordingGameObject(go1);
            PlumeRecorder.StartRecordingGameObject(go2);
            PlumeRecorder.RecordMarker("Start");
            await PlumeRecorder.StopRecording();
        });
    }
}