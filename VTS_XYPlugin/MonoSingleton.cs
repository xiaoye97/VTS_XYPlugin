using UnityEngine;

namespace VTS_XYPlugin
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }
                else
                {
                    GameObject go = new GameObject();
                    instance = go.AddComponent<T>();
                    go.name = $"[XYPlugin]{instance.GetType().Name}";
                    GameObject.DontDestroyOnLoad(go);
                    return instance;
                }
            }
        }

        public virtual void Init()
        {
        }
    }
}
