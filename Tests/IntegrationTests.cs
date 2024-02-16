using System.Collections;
using Cysharp.Threading.Tasks;
using PLUME.Core.Object.SafeRef;
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
            
            var objectSafeRefProvider = PlumeRecorder.Instance.Context.ObjectSafeRefProvider;

            var go1 = new GameObject("go1");
            var go2 = new GameObject("go2");
            var tRef1 = objectSafeRefProvider.GetOrCreateTypedObjectSafeRef(go1.transform);
            var tRef2 = objectSafeRefProvider.GetOrCreateTypedObjectSafeRef(go2.transform);

            var recordIdentifier = new RecordIdentifier("test");

            PlumeRecorder.StartRecording(recordIdentifier);
            PlumeRecorder.StartRecordingObject(tRef1);
            PlumeRecorder.StartRecordingObject(tRef2);
            PlumeRecorder.RecordMarker("Start");
            await PlumeRecorder.StopRecording();
        });
    }
}