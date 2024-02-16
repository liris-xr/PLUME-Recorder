using System;
using UnityEngine;
using Logger = PLUME.Core.Recorder.Logger;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PLUME.Core.Scripts
{
    [DisallowMultipleComponent]
    public class ApplicationPauseDetector : MonoBehaviour
    {
        public static event Action Paused;

        private static ApplicationPauseDetector _instance;

        static ApplicationPauseDetector()
        {
#if UNITY_EDITOR
            EditorApplication.pauseStateChanged += state =>
            {
                if (state == PauseState.Paused)
                {
                    Paused?.Invoke();
                }
            };
#endif
        }

        public void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                Logger.LogWarning("Multiple ApplicationPauseDetectors in scene. Destroying one.");
            }
            else
            {
                DontDestroyOnLoad(gameObject);
                _instance = this;
            }
        }

        private void OnApplicationPause(bool pausedStatus)
        {
            if (pausedStatus)
            {
                Paused?.Invoke();
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public static void EnsureExists()
        {
            if (_instance != null) return;
            var go = new GameObject("ApplicationPauseDetector");
            go.AddComponent<ApplicationPauseDetector>();
        }

        public static void Destroy()
        {
            if (_instance != null)
            {
                Destroy(_instance.gameObject);
            }
        }
    }
}