// By 宵夜97
using Lean.Gui;
using Lean.Pool;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VTS_XYPlugin_Common;
using System.Collections.Generic;

public class UIPageLiveRoom : MonoSingleton<UIPageLiveRoom>
{
    public RectTransform DanMuRT;
    public LeanGameObjectPool DanMuPool;
    public RectTransform GiftRT;
    public LeanGameObjectPool GiftPool;
    public RectTransform SCRT;
    public LeanGameObjectPool SCPool;
    public RectTransform JianZhangRT;
    public LeanGameObjectPool JianZhangPool;
    public RectTransform LogRT;
    public LeanGameObjectPool LogPool;
    public Text RenQiText;
    public Text WatchPoepleText;
    public LeanToggle OnlyShowJinGuaZiGiftToggle;
    public LeanToggle DanMuJiStateToggle;
    public int DanMuJiPID;
    private float checkDanMuJiCD;

    public static Dictionary<BJianDuiType, string> JianDuiColorStrDict = new Dictionary<BJianDuiType, string>()
    {
        { BJianDuiType.无, "999999" },
        { BJianDuiType.舰长, "5168fa" },
        { BJianDuiType.提督, "ab40fa" },
        { BJianDuiType.总督, "fa6743" },
    };

    private void Update()
    {
        checkDanMuJiCD -= Time.deltaTime;
        if (checkDanMuJiCD < 0)
        {
            checkDanMuJiCD = 3f;
            bool hasDanMuJi = false;
            // 每隔3秒检查弹幕姬进程是否存在
            if (DanMuJiPID != 0)
            {
                try
                {
                    var process = System.Diagnostics.Process.GetProcessById(DanMuJiPID);
                    if (process != null)
                    {
                        hasDanMuJi = true;
                    }
                }
                catch { }
            }
            DanMuJiStateToggle.On = hasDanMuJi;
        }
    }

    public void Refresh(XYPluginCache cache)
    {
        AddDanMu(cache);
        AddGift(cache);
        AddSC(cache);
        AddJianZhang(cache);
        AddLog(cache);
        if (cache.RenQiMessages.Count > 0)
        {
            var last = cache.RenQiMessages[cache.RenQiMessages.Count - 1];
            RenQiText.text = $"人气值:{last.人气}";
        }
        if (cache.WatchPeopleMessages.Count > 0)
        {
            var last = cache.WatchPeopleMessages[cache.WatchPeopleMessages.Count -1];
            WatchPoepleText.text = $"看过人数:{last.看过人数}";
        }
        DanMuJiPID = cache.BiliDanMuJiPID;
    }

    public string ReplaceUserName(string userName, bool isAdmin, BJianDuiType jianDuiType)
    {
        string result = "";
        if (isAdmin)
            result = $"<color=#ff0000>{userName}</color>";
        else
            result = $"<color=#{JianDuiColorStrDict[jianDuiType]}>{userName}</color>";
        return result;
    }

    public void AddDanMu(XYPluginCache cache)
    {
        foreach (var data in cache.DanMuMessages)
        {
            var obj = DanMuPool.Spawn(DanMuRT);
            string userName = ReplaceUserName(data.用户名, data.是否房管, data.舰队类型);
            string showText = $"{userName}：{data.弹幕}";
            showText = showText.Replace(" ", "\u00A0");
            obj.GetComponent<Text>().text = showText;
        }
    }

    public void AddGift(XYPluginCache cache)
    {
        foreach (var data in cache.GiftMessages)
        {
            if (OnlyShowJinGuaZiGiftToggle.On)
            {
                if (data.瓜子类型 == BGiftCoinType.银瓜子)
                {
                    continue;
                }
            }
            var obj = GiftPool.Spawn(GiftRT);
            string showText = $"<color=#999999>{data.用户名}</color> 赠送了 <color=#ec407a>{data.礼物名}</color> x<color=#29b6f6>{data.礼物数量}</color>个";
            if (data.瓜子类型 == BGiftCoinType.金瓜子)
            {
                showText += $" <color=#ff9800>[￥{data.瓜子数量 / 1000f}]</color>";
            }
            showText = showText.Replace(" ", "\u00A0");
            obj.GetComponent<Text>().text = showText;
        }
    }

    public void AddSC(XYPluginCache cache)
    {
        foreach (var data in cache.SCMessages)
        {
            var obj = SCPool.Spawn(SCRT);
            string showText = $"<color=#999999>{data.用户名}</color> <color=#ff9800>[￥{data.金额}]</color>：{data.SC}";
            showText = showText.Replace(" ", "\u00A0");
            obj.GetComponent<Text>().text = showText;
        }
    }

    public void AddJianZhang(XYPluginCache cache)
    {
        foreach (var data in cache.BuyJianDuiMessages)
        {
            var obj = JianZhangPool.Spawn(JianZhangRT);
            string showText = $"<color=#{JianDuiColorStrDict[data.舰长类型]}>{data.用户名}</color> 开通了<color=#{JianDuiColorStrDict[data.开通类型]}>{data.开通类型}</color> x<color=#29b6f6>{data.开通数量}</color>";
            showText = showText.Replace(" ", "\u00A0");
            obj.GetComponent<Text>().text = showText;
        }
    }

    public void AddLog(XYPluginCache cache)
    {
        foreach (var data in cache.WarningMessages)
        {
            var obj = LogPool.Spawn(LogRT);
            string showText = $"<color=#ff0000>超管警告:{data.警告信息}</color>";
            showText = showText.Replace(" ", "\u00A0");
            obj.GetComponent<Text>().text = showText;
        }
    }

    public void AddLogMessage(string log)
    {
        var obj = LogPool.Spawn(LogRT);
        string showText = $"{log}";
        showText = showText.Replace(" ", "\u00A0");
        obj.GetComponent<Text>().text = showText;
    }

    public void AddLogWarning(string log)
    {
        var obj = LogPool.Spawn(LogRT);
        string showText = $"<color=yellow>{log}</color>";
        showText = showText.Replace(" ", "\u00A0");
        obj.GetComponent<Text>().text = showText;
    }

    public void AddLogError(string log)
    {
        var obj = LogPool.Spawn(LogRT);
        string showText = $"<color=red>{log}</color>";
        showText = showText.Replace(" ", "\u00A0");
        obj.GetComponent<Text>().text = showText;
    }
}
