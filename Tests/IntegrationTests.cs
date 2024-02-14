using System.Collections;
using Cysharp.Threading.Tasks;
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
        public IEnumerator IntegrationTest() => UniTask.ToCoroutine(async () =>
        {
            var objectSafeRefProvider = new ObjectSafeRefProvider();

            var go1 = new GameObject("go1");
            var go2 = new GameObject("go2");
            var tRef1 = objectSafeRefProvider.GetOrCreateTypedObjectSafeRef(go1.transform);
            var tRef2 = objectSafeRefProvider.GetOrCreateTypedObjectSafeRef(go2.transform);

            var recordIdentifier = new RecordIdentifier("test");

            Recorder.Instance.Start(recordIdentifier);
            Recorder.Instance.StartRecordingObject(tRef1, true);
            Recorder.Instance.StartRecordingObject(tRef2, true);
            Recorder.Instance.RecordMarker("Start");
            await Recorder.Instance.Stop();
        });
    }
}