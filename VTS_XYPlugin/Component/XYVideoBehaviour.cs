using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace VTS_XYPlugin
{
    public class XYVideoBehaviour : MonoBehaviour
    {
        public SpriteRenderer SR;
        public VideoPlayer VideoPlayer;
        public Action<VideoPlayer> OnVideoStarted;
        public Action<VideoPlayer> OnVideoLoopPointReached;

        private void Awake()
        {
            VideoPlayer.started += VideoPlayer_started;
            VideoPlayer.loopPointReached += VideoPlayer_loopPointReached;
        }

        private void VideoPlayer_loopPointReached(VideoPlayer source)
        {
            if (OnVideoLoopPointReached != null)
            {
                OnVideoLoopPointReached(source);
            }
            if (!VideoPlayer.isLooping)
            {
                GameObject.Destroy(gameObject);
            }
        }

        private void VideoPlayer_started(VideoPlayer source)
        {
            if (OnVideoStarted != null)
            {
                OnVideoStarted(source);
            }
            SR.color = Color.white;
        }

        public void PlayVideo(string path, bool isLoop, int order)
        {
            SR.sortingOrder = order;
            VideoPlayer.isLooping = isLoop;
            VideoPlayer.source = VideoSource.Url;
            VideoPlayer.url = path;
            VideoPlayer.Play();
        }

        public void PlayVideo(VideoClip clip, bool isLoop, int order)
        {
            SR.sortingOrder = order;
            VideoPlayer.isLooping = isLoop;
            VideoPlayer.source = VideoSource.VideoClip;
            VideoPlayer.clip = clip;
            VideoPlayer.Play();
        }
    }
}
