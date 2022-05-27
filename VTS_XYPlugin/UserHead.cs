using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using VTS_XYPlugin_Common;

namespace VTS_XYPlugin
{
    public class UserHead
    {
        public Sprite sprite;
        public Gif gif;
        public int UserID;
        public ImageType ImageType;
        public string FilePath;

        public UserHead()
        {
        }

        public UserHead(int userID, string filePath)
        {
            UserID = userID;
            FilePath = filePath;
        }

        /// <summary>
        /// 加载图片(需要先赋值好路径)
        /// </summary>
        public void LoadImage()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                XYLog.LogError($"加载头像错误，未赋值文件路径");
                return;
            }
            if (!File.Exists(FilePath))
            {
                XYLog.LogError($"加载头像错误，文件 {FilePath} 不存在");
                return;
            }
            if (FilePath.EndsWith(".gif"))
            {
                ImageType = ImageType.GIF;
                gif = new Gif();
                gif.LoadGif(FilePath, true);
                //XYLog.LogMessage($"加载gif完毕,共{gif.frameDelays.Count}帧");
            }
            else
            {
                ImageType = ImageType.PNG;
                sprite = FileHelper.LoadSprite(FilePath, true);
                //XYLog.LogMessage($"加载sprite完毕, pixelsPerUnit:{sprite.pixelsPerUnit}");
            }
        }
    }
}
