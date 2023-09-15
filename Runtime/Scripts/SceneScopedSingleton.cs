using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PLUME
{
    public abstract class SceneScopedSingleton<T> : SceneScopedSingleton where T : SceneScopedSingleton<T>
    {
        public new static T GetInstance(Scene scene)
        {
            return (T)Instances[scene];
        }
    }

    public abstract class SceneScopedSingleton : MonoBehaviour
    {
        protected static readonly Dictionary<Scene, SceneScopedSingleton> Instances = new();

        public static SceneScopedSingleton GetInstance(Scene scene)
        {
            return Instances[scene];
        }

        private void Awake()
        {
            if (Instances.ContainsKey(gameObject.scene) && Instances[gameObject.scene] != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instances[gameObject.scene] = this;
            }
        }
    }
}