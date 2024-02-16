using System;
using UnityEngine;

namespace PLUME.Core.Scripts
{
    [DisallowMultipleComponent]
    public class ApplicationPauseDetector : MonoBehaviour
    {
        public static event Action Paused;

        private static ApplicationPauseDetector _instance;

        public static ApplicationPauseDetector Instance
        {
            get
            {
                if (_instance == null)
                {
                    EnsureExists();
                }

                return _instance;
            }
        }

        public void Awake()
        {
            if (Instance != null)
            {
                DestroyImmediate(gameObject);
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
            if (Instance == this)
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
                Destroy(Instance.gameObject);
            }
        }
    }
}