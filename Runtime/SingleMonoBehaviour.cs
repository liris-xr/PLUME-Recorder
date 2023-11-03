using UnityEngine;

namespace PLUME
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        public void Awake()
        {
            if (Instance != null && ReferenceEquals(Instance, this))
            {
                Debug.LogWarning($"{nameof(T)} already exists. Removing new instance.");
                Destroy(gameObject);
            }
            else
            {
                Instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}