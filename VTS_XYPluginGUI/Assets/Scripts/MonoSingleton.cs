using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    public static T Instance
    {
        get
        {
            return instance;
        }
    }

    private static T instance;
    public virtual void Awake()
    {
        instance = this as T;
    }
}
