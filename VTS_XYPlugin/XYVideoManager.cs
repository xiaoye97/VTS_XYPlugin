using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace VTS_XYPlugin
{
    public class XYVideoManager : MonoSingleton<XYVideoManager>
    {
        private GameObject VideoPrefab;

        public override void Init()
        {
            CreatePrefab();
        }

        public XYVideoBehaviour PlayVideo(string path, bool isLoop, int order)
        {
            var go = GameObject.Instantiate(VideoPrefab);
            var v = go.GetComponent<XYVideoBehaviour>();
            go.SetActive(true);
            v.PlayVideo(path, isLoop, order);
            return v;
        }

        public XYVideoBehaviour PlayVideo(VideoClip clip, bool isLoop, int order)
        {
            var go = GameObject.Instantiate(VideoPrefab);
            var v = go.GetComponent<XYVideoBehaviour>();
            go.SetActive(true);
            v.PlayVideo(clip, isLoop, order);
            return v;
        }

        private void CreatePrefab()
        {
            VideoPrefab = new GameObject("VideoPrefab");
            VideoPrefab.SetActive(false);
            VideoPrefab.transform.position = new Vector3(0, 0, 100);
            VideoPrefab.layer = 8;
            var v = VideoPrefab.AddComponent<XYVideoBehaviour>();
            v.SR = VideoPrefab.AddComponent<SpriteRenderer>();
            v.SR.sprite = ResourceUtils.LoadDLLSprite("Square.png");
            v.SR.color = Color.clear;
            v.VideoPlayer = VideoPrefab.AddComponent<VideoPlayer>();
            v.VideoPlayer.playOnAwake = false;
            v.VideoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            v.VideoPlayer.targetMaterialRenderer = v.SR;
            VideoPrefab.AddComponent<AutoFillSize>();
        }
    }
}
