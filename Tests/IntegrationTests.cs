using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using PLUME.Base.Module.Unity.Transform;
using PLUME.Core;
using PLUME.Core.Object;
using PLUME.Core.Object.SafeRef;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module;
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

            var recorder = new Recorder(recorderModules);

            ((IRecorderModule)transformRecorderModule).Create();
            recorder.Start();
            transformRecorderModule.TryStartRecordingObject(tRef1, true);
            transformRecorderModule.TryStartRecordingObject(tRef2, true);
        });
    }
}