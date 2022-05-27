// By 宵夜97
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VTS_XYPlugin_Common;
using Newtonsoft.Json;

public class XYCache : MonoSingleton<XYCache>
{
    /// <summary>
    /// 缓存的集合
    /// </summary>
    public XYPluginCache Cache = new XYPluginCache();

    public void Update()
    {
        while (XYNetClient.MsgQueue.Count > 0)
        {
            var msg = XYNetClient.MsgQueue.Dequeue();
            XYPluginCache cache = JsonConvert.DeserializeObject<XYPluginCache>(msg);
            if (cache != null)
            {
                HandleCache(cache);
            }
        }
    }

    public void HandleCache(XYPluginCache cache)
    {
        if (cache != null)
        {
            bool nowModelDataChanged = false;
            // 覆盖数据
            if (Cache.NowModelConfigFilePath != cache.NowModelConfigFilePath)
            {
                nowModelDataChanged = true;
                Cache.NowModelConfigFilePath = cache.NowModelConfigFilePath;
            }
            if (Cache.NowModelHotkeys.Count != cache.NowModelHotkeys.Count)
            {
                nowModelDataChanged = true;
                Cache.NowModelHotkeys = cache.NowModelHotkeys;
            }
            else
            {
                for (int i = 0; i < Cache.NowModelHotkeys.Count; i++)
                {
                    if (Cache.NowModelHotkeys[i] != cache.NowModelHotkeys[i])
                    {
                        nowModelDataChanged = true;
                        break;
                    }
                }
                if (nowModelDataChanged)
                {
                    Cache.NowModelHotkeys = cache.NowModelHotkeys;
                }
            }
            Cache.WatchPeopleMessages.AddRange(cache.WatchPeopleMessages);
            Cache.RenQiMessages.AddRange(cache.RenQiMessages);
            Cache.GiftMessages.AddRange(cache.GiftMessages);
            Cache.WarningMessages.AddRange(cache.WarningMessages);
            Cache.BuyJianDuiMessages.AddRange(cache.BuyJianDuiMessages);
            Cache.DanMuMessages.AddRange(cache.DanMuMessages);
            Cache.SCMessages.AddRange(cache.SCMessages);
            Cache.InstallExScripts.Clear();
            foreach (var script in cache.InstallExScripts)
            {
                Cache.InstallExScripts.Add(script);
            }
            // 刷新UI
            UIPageLiveRoom.Instance.Refresh(cache);
            UIPageSctipt.Instance.Refresh();
            // 如果模型的数据变了，则刷新动作绑定UI
            if (nowModelDataChanged)
            {
                UIPageActionBind.Instance.Refresh(true);
            }
        }
    }
}
