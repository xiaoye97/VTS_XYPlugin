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