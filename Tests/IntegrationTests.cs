using System.Collections;
using Cysharp.Threading.Tasks;
using PLUME.Recorder;
using PLUME.Recorder.Module;
using PLUME.Recorder.Module.Unity.Transform;
using PLUME.Recorder.Serializer;
using PLUME.Recorder.Serializer.Unity;
using Unity.Collections;
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
            
            var dataBuffer = new NativeByteBufferWriter(Allocator.Persistent);

            var transformRecorderModule = new TransformRecorderModule();
            var transformSampleSerializer = new TransformUpdatePositionSampleSerializer();

            var recorderModules = new IUnityRecorderModule[] { transformRecorderModule };
            var serializers = new TimestampedSampleSerializer[] { transformSampleSerializer };

            var unityFrameRecorder = new UnityFrameRecorder(recorderModules);
            var unityFrameSerializer = new UnityFrameSerializer(serializers);

            ((IRecorderModule)transformRecorderModule).Create();
            unityFrameRecorder.Start();
            transformRecorderModule.TryStartRecording(tRef1, true);
            transformRecorderModule.TryStartRecording(tRef2, true);

            var frameData = unityFrameRecorder.RecordFrame(0);
            await unityFrameSerializer.SerializeFrameAsync(frameData, dataBuffer);
            
            Debug.Log(dataBuffer.AsReadOnlyArray().Length);

            dataBuffer.Dispose();
        });
    }
}