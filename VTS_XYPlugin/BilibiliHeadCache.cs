using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using VTS_XYPlugin_Common;

namespace VTS_XYPlugin
{
    /// <summary>
    /// B站头像缓存
    /// </summary>
    public class BilibiliHeadCache : MonoSingleton<BilibiliHeadCache>
    {
        public Dictionary<string, string> HeadLinkDict = new Dictionary<string, string>();
        public Dictionary<string, UserHead> HeadDict = new Dictionary<string, UserHead>();
        private Queue<string> waitGetHeadUrlUsers = new Queue<string>();
        public static Queue<string> waitDownloadHeads = new Queue<string>();

        private float saveCD = 60f;
        private int lastSaveCount;

        // 访问用户空间的CD 设定为3秒
        private float accessUserSpaceCD;

        private void Update()
        {
            saveCD -= Time.deltaTime;
            accessUserSpaceCD -= Time.deltaTime;
            if (saveCD < 0)
            {
                saveCD = 60f;
                if (lastSaveCount != HeadLinkDict.Count)
                {
                    lastSaveCount = HeadLinkDict.Count;
                    FileHelper.SaveBiliHeadCache();
                }
            }
            if (accessUserSpaceCD < 0)
            {
                if (waitGetHeadUrlUsers.Count > 0)
                {
                    string userID = waitGetHeadUrlUsers.Dequeue();
                    // 如果在等待期间已经获取到此用户头像，则跳过
                    if (!HeadLinkDict.ContainsKey(userID))
                    {
                        accessUserSpaceCD = 3f;
                        string url = GetUserHeadUrl(userID);
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            HeadLinkDict[userID] = url;
                            // 如果不存在此文件，则下载
                            if (!File.Exists($"{XYPaths.BiliHeadDirPath}/{url.RemoveHttp()}"))
                            {
                                NetHelper.DownloadHead(url);
                            }
                        }
                    }
                }
            }
        }

        private void OnApplicationQuit()
        {
            FileHelper.SaveBiliHeadCache();
        }

        public override void Init()
        {
            FileHelper.LoadBiliHeadCache();
            lastSaveCount = HeadLinkDict.Count;

            // 开启头像下载线程
            ThreadStart childRef = new ThreadStart(DownloadHeadThread);
            Thread childThread = new Thread(childRef);
            childThread.Start();
        }

        private void DownloadHeadThread()
        {
            while (true)
            {
                if (waitDownloadHeads.Count > 0)
                {
                    string url = waitDownloadHeads.Dequeue();
                    NetHelper.DownloadHead(url);
                }
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// 获取用户头像
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public UserHead GetHead(string userID)
        {
            if (string.IsNullOrWhiteSpace(userID))
            {
                return null;
            }
            // 如果缓存里有，则使用缓存
            if (HeadDict.ContainsKey(userID))
            {
                //XYLog.LogMessage($"[获取头像][{userID}]图片缓存里有，直接用缓存");
                return HeadDict[userID];
            }
            // 如果缓存里没有但有记录链接，则查找本地文件
            if (HeadLinkDict.ContainsKey(userID))
            {
                //XYLog.LogMessage($"[获取头像][{userID}]链接缓存里有url，开始检查文件");
                string path = $"{XYPaths.BiliHeadDirPath}/{HeadLinkDict[userID]}";
                if (File.Exists(path))
                {
                    //XYLog.LogMessage($"[获取头像][{userID}]本地文件存在，直接加载图片");
                    return LoadAndCacheHead(userID, path);
                }
                else
                {
                    //XYLog.LogMessage($"[获取头像][{userID}]本地文件不存在，下载文件");
                    NetHelper.DownloadHead(HeadLinkDict[userID]);
                    if (File.Exists(path))
                    {
                        //XYLog.LogMessage($"[获取头像][{userID}]下载成功，加载图片");
                        return LoadAndCacheHead(userID, path);
                    }
                    else
                    {
                        //XYLog.LogMessage($"[获取头像][{userID}]下载失败");
                    }
                }
            }
            // 如果本地也没有，则需要下载
            // 如果当前CD为冷却状态并且队列没有等待抓取，则直接抓取
            if (accessUserSpaceCD < 0 && waitGetHeadUrlUsers.Count == 0)
            {
                //XYLog.LogMessage($"[获取头像][{userID}]当前CD为冷却状态并且队列没有等待抓取，直接抓取用户头像url");
                accessUserSpaceCD = 3f;
                string url = GetUserHeadUrl(userID);
                if (!string.IsNullOrWhiteSpace(url))
                {
                    //XYLog.LogMessage($"[获取头像][{userID}]抓取成功，检查文件");
                    HeadLinkDict[userID] = url;
                    string path = $"{XYPaths.BiliHeadDirPath}/{HeadLinkDict[userID].RemoveHttp()}";
                    if (File.Exists(path))
                    {
                        //XYLog.LogMessage($"[获取头像][{userID}]本地文件存在，直接加载图片");
                        return LoadAndCacheHead(userID, path);
                    }
                    else
                    {
                        //XYLog.LogMessage($"[获取头像][{userID}]本地文件不存在，下载文件");
                        NetHelper.DownloadHead(url);
                        if (File.Exists(path))
                        {
                            //XYLog.LogMessage($"[获取头像][{userID}]下载成功，加载图片");
                            return LoadAndCacheHead(userID, path);
                        }
                        else
                        {
                            //XYLog.LogMessage($"[获取头像][{userID}]下载失败");
                        }
                    }
                }
            }
            // 否则返回空使用默认图片，并在后台开启下载
            else
            {
                //XYLog.LogMessage($"[获取头像][{userID}]当前抓取队列不为空，将此次抓取请求加入队列，返回空头像");
                waitGetHeadUrlUsers.Enqueue(userID);
            }
            return null;
        }

        private UserHead LoadAndCacheHead(string userID, string path)
        {
            UserHead head = new UserHead(userID, path);
            head.LoadImage();
            HeadDict[userID] = head;
            return head;
        }

        /// <summary>
        /// 当收到礼物时检查是否缓存头像
        /// </summary>
        /// <param name="message"></param>
        public void OnRecvGift(BGiftMessage message)
        {
            string url = message.头像图片链接;
            if (HeadLinkDict.ContainsKey(message.用户ID))
            {
                // 如果缓存的数据和传入的数据不一致，说明需要更新
                if (HeadLinkDict[message.用户ID] != url)
                {
                    HeadLinkDict[message.用户ID] = url;
                }
            }
            else
            {
                HeadLinkDict[message.用户ID] = url;
            }
            if (XYPlugin.Instance.GlobalConfig.DownloadHeadOnRecvGift)
            {
                // 如果不存在此文件，则加入下载队列
                if (!File.Exists($"{XYPaths.BiliHeadDirPath}/{url.RemoveHttp()}"))
                {
                    waitDownloadHeads.Enqueue(url);
                }
            }
        }

        public string GetUserHeadUrl(string userID)
        {
            try
            {
                XYLog.LogMessage($"开始抓取用户{userID}的头像链接");
                string api = $"https://account.bilibili.com/api/member/getCardByMid?mid={userID}";
                string json = NetHelper.GetHttpResponse(api, 3000);
                JObject obj;
                obj = JObject.Parse(json);
                string url = obj["card"]["face"].ToString();
                XYLog.LogMessage(url);
                return url;
            }
            catch (Exception ex)
            {
                XYLog.LogError($"{ex}");
                return "";
            }
        }
    }
}