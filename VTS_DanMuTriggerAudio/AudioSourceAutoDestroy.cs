using UnityEngine;

namespace VTS_DanMuTriggerAudio
{
    public class AudioSourceAutoDestroy : MonoBehaviour
    {
        public AudioSource audioSource;
        public DanMuTriggerAudioConfig config;
        public bool autoDestroy;

        public void Update()
        {
            if (autoDestroy)
            {
                if (!audioSource.isPlaying)
                {
                    DanMuTriggerAudio.AudioSources[config.AudioFile].Remove(this);
                    Lean.Pool.LeanPool.Despawn(gameObject);
                }
            }
        }
    }
}
