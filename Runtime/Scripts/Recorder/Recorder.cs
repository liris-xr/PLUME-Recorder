using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using PLUME.Sample;
using PLUME.Sample.Common;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace PLUME
{
    [DisallowMultipleComponent]
    public class Recorder : SingletonMonoBehaviour<Recorder>, IDisposable
    {
        public static readonly RecorderVersion Version = new()
        {
            Name = "PLUME",
            Major = "alpha-1",
            Minor = "0",
            Patch = "0"
        };
        
        public string recordDirectory;
        public string recordPrefix = "record";
        public string recordIdentifier = System.Guid.NewGuid().ToString();
        
        public bool autoStart = true;
        public bool enableSamplePooling = true;

        public string extraMetadata;
        
        public int recordWriterBufferSize = 4096; // in bytes

        public CompressionLevel compressionLevel;
        
        private readonly HashSet<int> _recordedObjectsInstanceIds = new();

        private uint _nextProtobufSampleSeq;

        private RecordWriter _recordWriter;

        private IStartRecordingEventReceiver[] _startRecordingEventReceivers;
        private IStopRecordingEventReceiver[] _stopRecordingEventReceivers;
        private IStartRecordingObjectEventReceiver[] _startRecordingObjectEventReceivers;
        private IStopRecordingObjectEventReceiver[] _stopRecordingObjectEventReceivers;

        private readonly SamplePoolManager _samplePoolManager = new();

        public bool IsRecording { get; private set; }
        public RecorderClock Clock { get; } = new();

        [RuntimeInitializeOnLoadMethod]
        public static void OnInitialize()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

#if UNITY_STANDALONE_WIN
            Patcher.DoPatching();
#else
            // Android platforms can't be patched using Harmony because of IL2CPP
            var go = new GameObject();
            go.name = "Android event dispatcher";
            go.AddComponent<AndroidEventDispatcher>();
            DontDestroyOnLoad(go);
#endif
        }
        
        public new void Awake()
        {
            base.Awake();
            
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
                StartRecording();
        }

        public void OnDestroy()
        {
            StopRecording();
        }

        public bool StartRecording()
        {
            if (IsRecording)
                return false;
            
            _nextProtobufSampleSeq = 0;
            _recordedObjectsInstanceIds.Clear();

            if (!Directory.Exists(recordDirectory))
                Directory.CreateDirectory(recordDirectory);

            var recordFilepath = NewRecordFilePath(recordDirectory, recordPrefix);

            var createdAt = Timestamp.FromDateTime(DateTime.UtcNow);
            
            var recordMetadata = new RecordMetadata
            {
                Identifier = recordIdentifier,
                CreatedAt = createdAt,
                RecorderVersion = Version,
                ExtraMetadata = extraMetadata
            };

            _recordWriter = new RecordWriter(Clock, _samplePoolManager, recordFilepath, compressionLevel, recordMetadata, recordWriterBufferSize);

            if (!Stopwatch.IsHighResolution)
            {
                Debug.LogWarning(
                    "Your stopwatch is not high resolution. Timestamps may not be precise up to the millisecond.");
            }

            IsRecording = true;
            Clock.Restart();
            
            var recordHeader = new RecordHeader
            {
                Identifier = recordIdentifier,
                CreatedAt = createdAt,
                RecorderVersion = Version,
                ExtraMetadata = extraMetadata
            };

            RecordSample(recordHeader);
            
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
                $"Started recording using {Version.Name} Version {Version.Major}.{Version.Minor}.{Version.Patch}");
            return true;
        }

        private static string NewRecordFilePath(string recordDirectory, string prefix)
        {
            var formattedDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-sszz");
            var filenameBase = $"{prefix}_{formattedDateTime}";
            var fileName = $"{filenameBase}.plm";

            if (!File.Exists(fileName)) return Path.Join(recordDirectory, fileName);
            
            string suffixedFilename;
            var i = 1;

            do
            {
                suffixedFilename = $"{filenameBase}_{i}.plm";
                ++i;
            } while (File.Exists(suffixedFilename));

            return Path.Join(recordDirectory, suffixedFilename);
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
            
            Clock.Stop();
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
            var success = TryRecordSampleStamped(marker);
            return success;
        }

        public void RecordMarker(string label)
        {
            var marker = new Marker {Label = label};
            RecordSampleStamped(marker);
        }
        
        public bool TryRecordSampleStamped(IMessage samplePayload, long timestampOffset = 0)
        {
            try
            {
                RecordSampleStamped(samplePayload, timestampOffset);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public void RecordSampleStamped(IMessage samplePayload, long timestampOffset = 0)
        {
            if (!IsRecording)
                throw new Exception($"Recording sample but {nameof(Recorder)} is not running.");
            
            var time = (long) Clock.GetTimeInNanoseconds() + timestampOffset;

            if (time < 0)
                time = 0;

            UnpackedSample unpackedSampleStamped;
            
            if (enableSamplePooling)
            {
                unpackedSampleStamped = _samplePoolManager.GetUnpackedSampleStamped();
            }
            else
            {
                unpackedSampleStamped = new UnpackedSample();
                unpackedSampleStamped.Header = new SampleHeader();
            }
            
            unpackedSampleStamped.Header.Seq = _nextProtobufSampleSeq++;
            unpackedSampleStamped.Header.Time = (ulong) time;
            unpackedSampleStamped.Payload = samplePayload;
            _recordWriter.Write(unpackedSampleStamped);
        }
        
        public bool TryRecordSample(IMessage samplePayload)
        {
            try
            {
                RecordSample(samplePayload);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public void RecordSample(IMessage samplePayload)
        {
            if (!IsRecording)
                throw new Exception($"Recording sample but {nameof(Recorder)} is not running.");

            UnpackedSample unpackedSample;
            
            if (enableSamplePooling)
            {
                unpackedSample = _samplePoolManager.GetUnpackedSample();
            }
            else
            {
                unpackedSample = new UnpackedSample();
            }
            
            unpackedSample.Payload = samplePayload;
            _recordWriter.Write(unpackedSample);
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