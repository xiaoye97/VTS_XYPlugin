using UnityEngine;

namespace VTS_ParameterTriggerAudio
{
    public class AudioSourceAutoDestroy : MonoBehaviour
    {
        public AudioPlayer Player;
        public AudioSource audioSource;
        public bool autoDestroy;

        public void Update()
        {
            if (autoDestroy)
            {
                if (!audioSource.isPlaying)
                {
                    Lean.Pool.LeanPool.Despawn(gameObject);
                    Player.NowOneShotAudioCount--;
                }
            }
        }
    }
}