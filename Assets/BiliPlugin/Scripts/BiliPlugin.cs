
using VTS;
using VTS.Models;
using UnityEngine;
using UnityEngine.UI;
using VTS.Models.Impl;
using System.Collections;
using VTS.Networking.Impl;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class BiliPlugin : VTSPlugin
{
    public static BiliPlugin Instance;
    public BiliPython BiliPython;
    public DropItemManager DropItemManager;
    private ConnectState connectState;
    /// <summary>
    /// 连接状态
    /// </summary>
    public ConnectState ConnectState
    {
        get { return connectState; }
        set 
        {
            connectState = value;
            if (UIConnectState != null)
            {
                UIConnectState.NowState = connectState;
            }
        }
    }
    // 等待播放的动作
    public Queue<string> ActionQueue = new Queue<string>();
    public Dictionary<string, float> ActionCDDict = new Dictionary<string, float>();
    public float ActionCD;
    public float ActionDuration = 5f;

    #region UI
    public UIConnectState UIConnectState;
    public UIConnectRoom UIConnectRoom;
    public UIGiftPanel UIGiftPanel;
    public UILogPanel UILogPanel;
    public UIActionPanel UIActionPanel;
    public UIDropItemMiniPanel UIDropItemMiniPanel;
    public UIDropItemPanel UIDropItemPanel;
    public UIDropItemSettingPanel UIDropItemSettingPanel;
    #endregion

    void Start()
    {
        Instance = this;
        Application.targetFrameRate = 30;
        ConnectToVTS();
        BiliPython = new BiliPython();
        DropItemManager = new DropItemManager();
    }

    void Update()
    {
        if (BiliPython.dataQueue.Count > 0)
        {
            lock (BiliPython.dataQueue)
            {
                string data = BiliPython.dataQueue.Dequeue();
                Debug.Log(data);
                OnRecvData(data);
            }
        }
        // 动作CD
        if (ActionCD >= 0)
        {
            ActionCD -= Time.deltaTime;
        }
        //  动作触发CD
        List<string> keys = new List<string>();
        foreach (var key in ActionCDDict.Keys)
        {
            keys.Add(key);
        }
        for (int i = 0; i < keys.Count; i++)
        {
            ActionCDDict[keys[i]] -= Time.deltaTime;
        }
        if (ActionCD < 0)
        {
            if (ActionQueue.Count > 0)
            {
                string action = ActionQueue.Dequeue();
                var data = UIActionPanel.GetActionTriggerData(action);
                if (data != null && data.ActionTime > 0)
                {
                    ActionCD = data.ActionTime;
                }
                else
                {
                    ActionCD = ActionDuration;
                }
                UIActionPanel.TriggerHotkey(action);
            }
        }
    }

    public string TriggerAction(GiftData gift)
    {
        string result = "";
        ActionTriggerData resultData = null;
        foreach (var data in UIActionPanel.ActionTriggerDatas.datas)
        {
            switch (data.TriggerType)
            {
                case ActionTriggerType.收到特定礼物时触发:
                    if (gift.GiftType == GiftType.Gift)
                    {
                        if (gift.GiftName == data.GiftName)
                        {
                            result = data.ActionName;
                            resultData = data;
                        }
                    }
                    break;
                case ActionTriggerType.收到礼物金额满足条件时触发:
                    if (gift.GiftType == GiftType.Gift)
                    {
                        int battery = (int)(gift.Money * 10);
                        if (battery >= data.MinBattery && battery <= data.MaxBattery)
                        {
                            result = data.ActionName;
                            resultData = data;
                        }
                    }
                    break;
                case ActionTriggerType.收到SC时触发:
                    if (gift.GiftType == GiftType.SC)
                    {
                        result = data.ActionName;
                        resultData = data;
                    }
                    break;
                case ActionTriggerType.收到舰长时触发:
                    if (gift.GiftType == GiftType.Jian)
                    {
                        result = data.ActionName;
                        resultData = data;
                    }
                    break;
            }
        }

        if (!string.IsNullOrWhiteSpace(result))
        {
            // 如果没有冷却，则直接跳过
            if (IsCDCooling(result))
            {
                ActionQueue.Enqueue(result);
                ActionCDDict[result] = resultData.TriggerCD;
            }
        }
        return result;
    }

    /// <summary>
    /// 检查是否结束冷却
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    private bool IsCDCooling(string action)
    {
        if (ActionCDDict.ContainsKey(action))
        {
            return ActionCDDict[action] <= 0;
        }
        else
        {
            return true;
        }
    }

    // 人气  f'R[{client.room_id}] 当前人气: {message.popularity}'
    // 弹幕  f'D[{client.room_id}] {message.uname}: {message.msg}'
    // 礼物  f'G[{client.room_id}] {message.uname} 赠送了 {message.gift_name}x{message.num}' f' ({message.coin_type} 瓜子 x {message.total_coin})'
    // 舰长  f'J[{client.room_id}] {message.username} 购买了 {message.gift_name}'
    // SC    f'S[{client.room_id}] 醒目留言 ￥{message.price} {message.uname}：{message.message}'
    public void OnRecvData(string data)
    {
        string[] danmu_msg = data.Split("$#**#$");
        string log = "";
        switch (data[0])
        {
            // 收到弹幕
            case 'D':
                log = $"{danmu_msg[1]} 发送弹幕: {danmu_msg[2]}";
                break;
            // 收到礼物
            case 'G':
                GiftData g = new GiftData();
                g.GiftType = GiftType.Gift;
                g.UserName = danmu_msg[1];
                g.GiftName = danmu_msg[2];
                g.GiftCount = int.Parse(danmu_msg[3]);
                log = $"{danmu_msg[1]} 赠送了 {danmu_msg[2]}x{danmu_msg[3]}";
                if (danmu_msg[4] == "gold")
                {
                    int guaZi = int.Parse(danmu_msg[5]);
                    float yuan = guaZi / 1000f;
                    g.Money = yuan;
                    log += $" (￥{yuan})";
                }
                g.ActionName = TriggerAction(g);
                if (DropItemManager.DropItemDataDict.ContainsKey(g.GiftName))
                {
                    SendDropItem(g.GiftName, g.GiftCount, (v) => { }, (e) => { });
                }
                UIGiftPanel.OnGift(g);
                break;
            // 有人上舰
            case 'J':
                GiftData j = new GiftData();
                j.GiftType = GiftType.Jian;
                j.UserName = danmu_msg[1];
                j.GiftName = danmu_msg[2];
                j.ActionName = TriggerAction(j);
                SendDropItem(j.GiftName, 1, (v) => { }, (e) => { });
                UIGiftPanel.OnGift(j);
                log = $"{danmu_msg[1]} 开通了 {danmu_msg[2]}";
                break;
            // SC
            case 'S':
                GiftData sc = new GiftData();
                sc.GiftType = GiftType.SC;
                sc.UserName = danmu_msg[2];
                sc.Money = float.Parse(danmu_msg[1]);
                sc.GiftMsg = danmu_msg[3];
                sc.ActionName = TriggerAction(sc);
                UIGiftPanel.OnGift(sc);
                log = $"发送了醒目留言 ￥{danmu_msg[1]} {danmu_msg[2]}：{danmu_msg[3]}";
                break;
            case 'R':
                log = $"直播间当前人气:{danmu_msg[1]}";
                break;
        }
        if (!string.IsNullOrWhiteSpace(log))
        {
            BiliPlugin.Log(log);
        }
        else
        {
            BiliPlugin.Log($"接收到数据:{data}");
        }
    }

    [Button]
    public static void Log(string msg, LogType logType = LogType.Log)
    {
        if (Instance != null && Instance.UILogPanel != null)
        {
            Instance.UILogPanel.Log(msg, logType);
        }
        switch (logType)
        {
            case LogType.Log:
                Debug.Log(msg);
                break;
            case LogType.Warning:
                Debug.LogWarning(msg);
                break;
            case LogType.Error:
                Debug.LogError(msg);
                break;
            case LogType.Exception:
                Debug.LogError(msg);
                break;
            case LogType.Assert:
                Debug.LogAssertion(msg);
                break;
        }
    }

    private void OnApplicationQuit()
    {
        if (BiliPython != null)
        {
            BiliPython.OnApplicationQuit();
        }
    }

    public void ConnectToVTS()
    {
        ConnectState = ConnectState.Connecting;
        Initialize(new WebSocketSharpImpl(), new JsonUtilityImpl(), new TokenStorageImpl(),
        () =>
        {
            Debug.Log("已连接!");
            ConnectState = ConnectState.Connected;
            // 连接成功后，发送数据
            DropItemManager.SaveAndSendCollider();
            DropItemManager.SaveDropSetting();
        },
        () =>
        {
            Debug.LogWarning("已断开连接!");
            ConnectState = ConnectState.Disconnected;
        },
        () =>
        {
            Debug.LogError("连接发生错误!");
            ConnectState = ConnectState.Error;
        });
    }
}
