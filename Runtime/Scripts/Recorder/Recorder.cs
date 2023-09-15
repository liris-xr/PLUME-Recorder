using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Google.Protobuf;
using PLUME.Guid;
using PLUME.Sample;
using PLUME.Sample.Common;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace PLUME
{
    [DisallowMultipleComponent]
    public class Recorder : SingletonMonoBehaviour<Recorder>, IDisposable
    {
        public AssetsGuidRegistry assetsGuidRegistry;
        
        public string recordDirectory;
        public string recordPrefix = "record";
        public string recordIdentifier = System.Guid.NewGuid().ToString();

        public bool autoStart = true;
        public bool enableSamplePooling = true;

        public int recordWriterBufferSize = 4096; // in bytes
        
        private Stopwatch _recorderClock;

        private readonly HashSet<int> _recordedObjectsInstanceIds = new();

        private uint _nextProtobufSampleSeq;

        private ThreadedRecordWriter _recordWriter;

        private IStartRecordingEventReceiver[] _startRecordingEventReceivers;
        private IStopRecordingEventReceiver[] _stopRecordingEventReceivers;
        private IStartRecordingObjectEventReceiver[] _startRecordingObjectEventReceivers;
        private IStopRecordingObjectEventReceiver[] _stopRecordingObjectEventReceivers;

        private readonly SamplePoolManager _samplePoolManager = new();

        public bool IsRecording { get; private set; }

        public new void Awake()
        {
            base.Awake();

            _recorderClock = new Stopwatch();

            _startRecordingEventReceivers =
                FindObjectsOfType<Object>(true).OfType<IStartRecordingEventReceiver>().ToArray();
            _stopRecordingEventReceivers =
                FindObjectsOfType<Object>(true).OfType<IStopRecordingEventReceiver>().ToArray();
            _startRecordingObjectEventReceivers =
                FindObjectsOfType<Object>(true).OfType<IStartRecordingObjectEventReceiver>().ToArray();
            _stopRecordingObjectEventReceivers =
                FindObjectsOfType<Object>(true).OfType<IStopRecordingObjectEventReceiver>().ToArray();

            recordDirectory = Application.persistentDataPath;
        }

        public void Start()
        {
            if (autoStart)
                StartRecording(recordIdentifier);
        }

        public void OnDestroy()
        {
            StopRecording();
        }

        public bool StartRecording(string recordIdentifier)
        {
            if (IsRecording)
                return false;
            
            _nextProtobufSampleSeq = 0;
            _recordedObjectsInstanceIds.Clear();

            if (!Directory.Exists(recordDirectory))
                Directory.CreateDirectory(recordDirectory);

            var recordFilepath =
                Path.Join(recordDirectory, FormatFilename(recordPrefix, "gz"));

            _recordWriter = new ThreadedRecordWriter(_samplePoolManager, recordFilepath, recordIdentifier, recordWriterBufferSize);

            if (!Stopwatch.IsHighResolution)
            {
                Debug.LogWarning(
                    "Your stopwatch is not high resolution. Timestamps may not be precise up to the millisecond.");
            }

            IsRecording = true;
            _recorderClock.Reset();
            _recorderClock.Start();

            foreach (var startRecordingEventReceiver in _startRecordingEventReceivers)
                startRecordingEventReceiver.OnStartRecording();

            var allComponents = FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var allGameObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            foreach (var go in allGameObjects)
            {
                TryStartRecordingObject(go);
            }
            
            foreach (var component in allComponents)
            {
                TryStartRecordingObject(component);
            }

            ObjectEvents.OnCreate += TryStartRecordingObject;
            ObjectEvents.OnDestroy += TryStopRecordingObject;

            Debug.Log(
                $"Started recording using {Plume.Version.Name} Version {Plume.Version.Major}.{Plume.Version.Minor}.{Plume.Version.Patch}");
            return true;
        }

        private string FormatFilename(string prefix, string extension)
        {
            var formattedDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-sszz");
            var filenameBase = $"{prefix}_{formattedDateTime}";
            var fileName = $"{filenameBase}.{extension}";

            if (!File.Exists(fileName)) return fileName;

            Debug.LogWarning($"File '{fileName}' already exists. Adding a suffix.");

            string suffixedFilename;
            var i = 1;

            do
            {
                suffixedFilename = $"{filenameBase}_{i}.{extension}";
                ++i;
            } while (File.Exists(suffixedFilename));

            return suffixedFilename;
        }

        public bool StopRecording()
        {
            if (!IsRecording)
                return false;

            foreach (var stopRecordingEventReceiver in _stopRecordingEventReceivers)
                stopRecordingEventReceiver.OnStopRecording();

            ObjectEvents.OnCreate -= TryStartRecordingObject;
            ObjectEvents.OnDestroy -= TryStopRecordingObject;

            var allComponents = FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var allGameObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            foreach (var component in allComponents)
            {
                TryStopRecordingObject(component.GetInstanceID());
            }
            
            foreach (var go in allGameObjects)
            {
                TryStopRecordingObject(go.GetInstanceID());
            }

            _recorderClock.Stop();
            
            _recordWriter.Close();

            IsRecording = false;
            return true;
        }

        private void TryStartRecordingObject(Object obj)
        {
            if (obj == null)
                return;
        
            if (!IsRecording)
                return;

            if (_recordedObjectsInstanceIds.Contains(obj.GetInstanceID()))
                return;

            // Don't record DontDestroyOnLoad objects
            if (obj is GameObject g && g.scene.buildIndex == -1)
                return;
            
            if (obj is Component c && c.gameObject.scene.buildIndex == -1)
                return;
            
            // Some object might be created by the Editor but we don't want to track them, for instance Pre Render Light, preview camera etc
            var dontSave = ((int) obj.hideFlags & (int) HideFlags.DontSave) > 0;

            if (dontSave)
                return;

            _recordedObjectsInstanceIds.Add(obj.GetInstanceID());

            foreach (var startRecordingObjectEventReceiver in _startRecordingObjectEventReceivers)
                startRecordingObjectEventReceiver.OnStartRecordingObject(obj);
        }

        private void TryStopRecordingObject(int objectInstanceId)
        {
            if (!IsRecording)
                return;

            if (!_recordedObjectsInstanceIds.Contains(objectInstanceId))
                return;

            _recordedObjectsInstanceIds.Remove(objectInstanceId);

            foreach (var stopRecordingObjectEventReceiver in _stopRecordingObjectEventReceivers)
                stopRecordingObjectEventReceiver.OnStopRecordingObject(objectInstanceId);
        }

        public bool TryRecordMaker(string label)
        {
            var marker = new Marker {Label = label};
            var success = TryRecordSample(marker);
            return success;
        }

        public void RecordMarker(string label)
        {
            var marker = new Marker {Label = label};
            RecordSample(marker);
        }
        
        public bool TryRecordSample(IMessage samplePayload, long timestampOffset = 0)
        {
            try
            {
                RecordSample(samplePayload, timestampOffset);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public void RecordSample(IMessage samplePayload, long timestampOffset = 0)
        {
            if (!IsRecording)
                throw new Exception($"Recording sample but {nameof(Recorder)} is not running.");

            var time = (long) GetTimeInNanoseconds() + timestampOffset;

            if (time < 0)
                time = 0;

            UnpackedSample unpackedSample;
            
            if (enableSamplePooling)
            {
                unpackedSample = _samplePoolManager.GetUnpackedSample();
            }
            else
            {
                unpackedSample = new UnpackedSample();
                unpackedSample.Header = new SampleHeader();
            }
            
            unpackedSample.Header.Seq = _nextProtobufSampleSeq++;
            unpackedSample.Header.Time = (ulong) time;
            unpackedSample.Payload = samplePayload;
            _recordWriter.Write(unpackedSample);
        }

        public ulong GetTimeInNanoseconds()
        {
            if (!_recorderClock.IsRunning)
            {
                throw new Exception("Recorder clock is not running.");
            }

            return (ulong) _recorderClock.ElapsedTicks * (ulong) (1_000_000_000 / Stopwatch.Frequency);
        }

        public SamplePoolManager GetSamplePoolManager()
        {
            return _samplePoolManager;
        }

        public void Dispose()
        {
            _recordWriter?.Dispose();
        }
    }
}