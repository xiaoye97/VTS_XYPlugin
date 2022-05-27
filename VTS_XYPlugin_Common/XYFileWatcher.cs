using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VTS_XYPlugin_Common
{
    public class XYFileWatcher
    {
        public string FilePath;
        DateTime lastWriteTime;
        float checkCD;
        bool fileExists;
        public Action OnFileCreated;
        public Action OnFileModified;
        public Action OnFileDeleted;
        // 忽略一次文件修改的事件触发，用于自己写文件时使用
        public bool IgnoreOnceModify;

        public XYFileWatcher(string path)
        {
            FilePath = path;
            FileInfo info = new FileInfo(FilePath);
            fileExists = info.Exists;
            if (fileExists)
            {
                lastWriteTime = info.LastWriteTime;
            }
        }

        /// <summary>
        /// 需要从Mono的Update调用此方法
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            checkCD -= deltaTime;
            if (checkCD < 0)
            {
                checkCD = 1f;
                Check();
            }
        }

        private void Check()
        {
            FileInfo info = new FileInfo(FilePath);
            // 如果文件存在，则检查文件是否被删除或者修改
            if (fileExists)
            {
                // 如果不存在则说明被删除
                if (!info.Exists)
                {
                    if (OnFileDeleted != null)
                    {
                        OnFileDeleted();
                    }
                }
                // 如果依旧存在则检查最后写入时间
                else
                {
                    // 如果最后写入时间大于当前记录的时间，则说明被修改了
                    if (info.LastWriteTime > lastWriteTime)
                    {
                        lastWriteTime = info.LastWriteTime;
                        if (IgnoreOnceModify)
                        {
                            IgnoreOnceModify = false;
                        }
                        else
                        {
                            if (OnFileModified != null)
                            {
                                OnFileModified();
                            }
                        }
                    }
                }
            }
            // 如果文件不存在，则检查文件是否被创建
            else
            {
                if (info.Exists)
                {
                    lastWriteTime = info.LastWriteTime;
                    if (OnFileCreated != null)
                    {
                        OnFileCreated();
                    }
                }
            }
            fileExists = info.Exists;
        }
    }
}
