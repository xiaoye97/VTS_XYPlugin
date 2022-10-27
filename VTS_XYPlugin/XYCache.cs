using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using VTS_XYPlugin_Common;

namespace VTS_XYPlugin
{
    public class XYCache : MonoSingleton<XYCache>
    {
        public override void Init()
        {
            BDanMuMessageListener = MessageCenter.Instance.Register<BDanMuMessage>(OnBDanMuMessage);
            BGiftMessageListener = MessageCenter.Instance.Register<BGiftMessage>(OnBGiftMessage);
            BWatchPeopleMessageListener = MessageCenter.Instance.Register<BWatchPeopleMessage>(OnBWatchPeopleMessage);
            BRenQiMessageListener = MessageCenter.Instance.Register<BRenQiMessage>(OnBRenQiMessage);
            BBuyJianDuiMessageListener = MessageCenter.Instance.Register<BBuyJianDuiMessage>(OnBBuyJianDuiMessage);
            BSCMessageListener = MessageCenter.Instance.Register<BSCMessage>(OnBSCMessage);
            BWarningMessageListener = MessageCenter.Instance.Register<BWarningMessage>(OnBWarningMessage);
        }

        private void Update()
        {
            sendCacheCD -= Time.deltaTime;
            if (sendCacheCD < 0)
            {
                if (PluginCache.HasData)
                {
                    sendCacheCD = 3f;
                    if (XYModelManager.Instance.modelConfigWatcher != null)
                    {
                        // 记下模型信息
                        PluginCache.NowModelConfigFilePath = XYModelManager.Instance.modelConfigWatcher.FilePath;
                        // 记下快捷键信息
                        PluginCache.NowModelHotkeys.Clear();
                        foreach (var hotkey in XYModelManager.Instance.NowModelDef.Hotkeys)
                        {
                            if (!string.IsNullOrWhiteSpace(hotkey.Name))
                            {
                                if (!PluginCache.NowModelHotkeys.Contains(hotkey.Name))
                                {
                                    PluginCache.NowModelHotkeys.Add(hotkey.Name);
                                }
                            }
                        }
                        // 记下扩展插件信息
                        PluginCache.InstallExScripts.Clear();
                        foreach (var script in XYExScriptManager.Instance.AllExScripts)
                        {
                            PluginCache.InstallExScripts.Add(script);
                        }
                    }
                    if (Bilibili.Instance != null && Bilibili.Instance.process != null)
                    {
                        PluginCache.BiliDanMuJiPID = Bilibili.Instance.process.Id;
                    }
                    string json = JsonConvert.SerializeObject(PluginCache);
                    //XYLog.LogMessage(json);
                    XYPlugin.Instance.ServerSkt.ServerBroadcast(new PENet.NetMsg() { text = json });
                    PluginCache = new XYPluginCache();
                }
            }
            while (GUICacheQueue.Count > 0)
            {
                string str = GUICacheQueue.Dequeue();
                if (!string.IsNullOrWhiteSpace(str))
                {
                    try
                    {
                        var cache = JsonConvert.DeserializeObject<XYGUICache>(str);
                        if (cache != null)
                        {
                            HandleGUICache(cache);
                        }
                    }
                    catch
                    { }
                }
            }
        }

        #region 插件向GUI

        /// <summary>
        /// 需要向GUI发送的数据缓存
        /// </summary>
        public XYPluginCache PluginCache = new XYPluginCache();

        private float sendCacheCD;

        private MessageListener BDanMuMessageListener;
        private MessageListener BGiftMessageListener;
        private MessageListener BWatchPeopleMessageListener;
        private MessageListener BRenQiMessageListener;
        private MessageListener BBuyJianDuiMessageListener;
        private MessageListener BSCMessageListener;
        private MessageListener BWarningMessageListener;

        private void OnBDanMuMessage(object message)
        {
            PluginCache.DanMuMessages.Add(message as BDanMuMessage);
            PluginCache.HasData = true;
        }

        private void OnBGiftMessage(object message)
        {
            PluginCache.GiftMessages.Add(message as BGiftMessage);
            PluginCache.HasData = true;
        }

        private void OnBWatchPeopleMessage(object message)
        {
            PluginCache.WatchPeopleMessages.Add(message as BWatchPeopleMessage);
            PluginCache.HasData = true;
        }

        private void OnBRenQiMessage(object message)
        {
            PluginCache.RenQiMessages.Add(message as BRenQiMessage);
            PluginCache.HasData = true;
        }

        private void OnBBuyJianDuiMessage(object message)
        {
            PluginCache.BuyJianDuiMessages.Add(message as BBuyJianDuiMessage);
            PluginCache.HasData = true;
        }

        private void OnBSCMessage(object message)
        {
            PluginCache.SCMessages.Add(message as BSCMessage);
            PluginCache.HasData = true;
        }

        private void OnBWarningMessage(object message)
        {
            PluginCache.WarningMessages.Add(message as BWarningMessage);
            PluginCache.HasData = true;
        }

        #endregion 插件向GUI

        #region GUI向插件

        public static Queue<string> GUICacheQueue = new Queue<string>();

        public void HandleGUICache(XYGUICache cache)
        {
            // 测试掉落
            if (cache.TestDrops != null && cache.TestDrops.Count > 0)
            {
                foreach (var drop in cache.TestDrops)
                {
                    var dropItem = XYDropManager.Instance.SearchDropItemByDropItemName(drop.Name);
                    //XYDropManager.Instance.StartDrop(dropItem, drop.TriggerCount, 1306433);
                    XYDropManager.Instance.StartDrop(dropItem, drop.TriggerCount, "1306433");
                }
            }
            if (cache.TestTriggerHotkeys != null && cache.TestTriggerHotkeys.Count > 0)
            {
                foreach (var hotkey in cache.TestTriggerHotkeys)
                {
                    XYHotkeyManager.Instance.TriggerHotkeyByNameNow(hotkey);
                }
            }
        }

        #endregion GUI向插件
    }
}