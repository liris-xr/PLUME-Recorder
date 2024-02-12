using System.Collections;
using PLUME.Base;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using UnityEngine;
using UnityEngine.TestTools;

namespace PLUME.Tests
{
    public class IntegrationTests
    {
        [UnityTest]
        public IEnumerator IntegrationTest()
        {
            var objectSafeRefProvider = new ObjectSafeRefProvider();

            var go1 = new GameObject("go1");
            var go2 = new GameObject("go2");
            var tRef1 = objectSafeRefProvider.GetOrCreateTypedObjectSafeRef(go1.transform);
            var tRef2 = objectSafeRefProvider.GetOrCreateTypedObjectSafeRef(go2.transform);
            
            PlumeRecorder.Instance.Start();
            PlumeRecorder.Instance.StartRecordingObject(tRef1, true);
            PlumeRecorder.Instance.StartRecordingObject(tRef2, true);
            PlumeRecorder.Instance.RecordMarker("Start");
            PlumeRecorder.Instance.Stop();

            yield return null;
        }

        // [UnityTest]
        // public IEnumerator IntegrationTest() => UniTask.ToCoroutine(async () =>
        // {
        //     var objectSafeRefProvider = new ObjectSafeRefProvider();
        //
        //     var go1 = new GameObject("go1");
        //     var go2 = new GameObject("go2");
        //     var tRef1 = objectSafeRefProvider.GetOrCreateTypedObjectSafeRef(go1.transform);
        //     var tRef2 = objectSafeRefProvider.GetOrCreateTypedObjectSafeRef(go2.transform);
        //     
        //     PlumeRecorder.Instance.Start();
        //     PlumeRecorder.Instance.StartRecordingObject(tRef1, true);
        //     PlumeRecorder.Instance.StartRecordingObject(tRef2, true);
        //     PlumeRecorder.Instance.RecordMarker("Start");
        //     PlumeRecorder.Instance.Stop();
        // });
    }
}