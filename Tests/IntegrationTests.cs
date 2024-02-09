using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using PLUME.Recorder;
using PLUME.Recorder.Module;
using PLUME.Recorder.Module.Unity.Transform;
using PLUME.Sample.Unity;
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

            try
            {
                SampleTypeUrlManager.RegisterTypeUrl("fr.liris.plume/" + TransformUpdatePosition.Descriptor.FullName);
            }
            catch (Exception)
            {
                // ignored
            }

            var transformRecorderModule = new TransformRecorderModule();
            var recorderModules = new IRecorderModule[] { transformRecorderModule };

            var recorder = new Recorder.Recorder(recorderModules);

            ((IRecorderModule)transformRecorderModule).Create();
            recorder.Start();
            transformRecorderModule.TryStartRecordingObject(tRef1, true);
            transformRecorderModule.TryStartRecordingObject(tRef2, true);
        });
    }
}