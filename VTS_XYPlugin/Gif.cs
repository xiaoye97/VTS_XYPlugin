using System.Collections.Generic;
using ThreeDISevenZeroR.UnityGifDecoder;
using UnityEngine;

namespace VTS_XYPlugin
{
    public class Gif
    {
        public Sprite firstFrameSprite;
        public Texture2D firstFrameTex;
        public List<Sprite> frameSprites = new List<Sprite>();
        public List<Texture2D> frameTexs = new List<Texture2D>();
        public List<float> frameDelays = new List<float>();
        private float perFrameTime = 0.06f;
        private float totalAnimTime;

        public Sprite GetNowSprite()
        {
            float animJinDu = Time.time % totalAnimTime;
            int index = (int)(animJinDu / perFrameTime);
            return frameSprites[index];
        }

        public Texture2D GetNowTex()
        {
            float animJinDu = Time.time % totalAnimTime;
            int index = (int)(animJinDu / perFrameTime);
            return frameTexs[index];
        }

        public void LoadGif(string path, bool setPerUnit = false)
        {
            int index = 0;
            using (var gifStream = new GifStream(path))
            {
                while (gifStream.HasMoreData)
                {
                    switch (gifStream.CurrentToken)
                    {
                        case GifStream.Token.Image:
                            var image = gifStream.ReadImage();
                            var frame = new Texture2D(
                                gifStream.Header.width,
                                gifStream.Header.height,
                                TextureFormat.ARGB32, false);
                            frame.SetPixels32(image.colors);
                            frame.Apply();
                            frameTexs.Add(frame);
                            frameDelays.Add(image.SafeDelaySeconds);
                            Sprite sprite = null;
                            if (setPerUnit)
                            {
                                sprite = Sprite.Create(frame, new Rect(0, 0, frame.width, frame.height), new Vector2(0.5f, 0.5f), frame.width);
                            }
                            else
                            {
                                sprite = Sprite.Create(frame, new Rect(0, 0, frame.width, frame.height), new Vector2(0.5f, 0.5f));
                            }
                            frameSprites.Add(sprite);
                            if (index == 0)
                            {
                                firstFrameSprite = sprite;
                                firstFrameTex = frame;
                            }
                            index++;
                            break;

                        default:
                            gifStream.SkipToken();
                            break;
                    }
                }
                totalAnimTime = frameDelays.Count * perFrameTime;
            }
        }
    }
}