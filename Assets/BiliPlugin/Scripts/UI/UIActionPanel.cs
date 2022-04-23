using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VTS.Models;
using VTS.Models.Impl;
using SuperScrollView;

public class UIActionPanel : MonoBehaviour
{
    public Button GetHotKeyBtn;
    public Button TestHotKeyBtn;
    public Button CreateBindBtn;
    public TMP_Dropdown AllActionDP;
    public LoopListView2 ListView;
    public GameObject CreateBindPrefab;
    public List<HotkeyData> HotkeyDataList = new List<HotkeyData>();
    public ActionTriggerDatas ActionTriggerDatas;
    public TextMeshProUGUI WaitPlayActionText;
    public Button ClearActionBtn;

    void Start()
    {
        GetHotKeyBtn.onClick.AddListener(OnClickGetHotKeyBtn);
        TestHotKeyBtn.onClick.AddListener(OnClickTestHotKeyBtn);
        CreateBindBtn.onClick.AddListener(OnClickCreateBindBtn);
        ClearActionBtn.onClick.AddListener(OnClickClearBtn);
        ActionTriggerDatas = ES3.Load<ActionTriggerDatas>("ActionTriggerDatas", new ActionTriggerDatas());
        ListView.InitListView(ActionTriggerDatas.datas.Count, OnGetItemByIndex);
    }

    private void Update()
    {
        WaitPlayActionText.text = $"待播放动作:{BiliPlugin.Instance.ActionQueue.Count}";
    }

    public void OnClickGetHotKeyBtn()
    {
        BiliPlugin.Log("开始获取快捷键，如等待几秒后没有获取到请重试");
        BiliPlugin.Instance.GetHotkeysInCurrentModel(
        null,
        (r) =>
        {
            HotkeyDataList = new List<HotkeyData>(r.data.availableHotkeys);
            //BiliPlugin.Log(new JsonUtilityImpl().ToJson(r));
            if (HotkeyDataList.Count == 0)
            {
                BiliPlugin.Log("没有找到快捷键", LogType.Warning);
            }
            else
            {
                BiliPlugin.Log($"{HotkeyDataList.Count}个快捷键已导入");
            }
            AllActionDP.ClearOptions();
            AllActionDP.AddOptions(new List<string>() { "无动作" });
            AllActionDP.value = 0;
            foreach (HotkeyData data in HotkeyDataList)
            {
                AllActionDP.AddOptions(new List<string>() { data.name });
            }
        },
        (e) => 
        {
            BiliPlugin.Log(e.data.message, LogType.Error);
        }
        );
    }

    public HotkeyData TriggerHotkey(string key)
    {
        BiliPlugin.Log($"尝试触发热键{key}");
        foreach (var hotkey in HotkeyDataList)
        {
            if (hotkey.name == key)
            {
                BiliPlugin.Instance.TriggerHotkey(hotkey.hotkeyID,
                        (r) => { BiliPlugin.Log($"触发热键{key}成功"); },
                        e => { BiliPlugin.Log("热键不存在，请重新获取", LogType.Warning); }
                        );
                return hotkey;
            }
        }
        BiliPlugin.Log("热键不存在，请重新设置", LogType.Warning);
        return null;
    }

    private void OnClickTestHotKeyBtn()
    {
        string key = AllActionDP.options[AllActionDP.value].text;
        TriggerHotkey(key);
    }

    private void OnClickCreateBindBtn()
    {
        if (AllActionDP.options.Count < 2)
        {
            BiliPlugin.Log("请先获取快捷键再进行绑定", LogType.Warning);
        }
        else
        {
            GameObject.Instantiate(CreateBindPrefab, transform.parent);
        }
    }

    private void OnClickClearBtn()
    {
        lock (BiliPlugin.Instance.ActionQueue)
        {
            BiliPlugin.Instance.ActionQueue.Clear();
        }
    }

    LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (index < 0 || index >= ActionTriggerDatas.datas.Count)
        {
            return null;
        }

        ActionTriggerData data = ActionTriggerDatas.datas[index];
        if (data == null)
        {
            return null;
        }
        LoopListViewItem2 item = listView.NewListViewItem("ActionBindPrefab");
        UIActionBindItem itemScript = item.GetComponent<UIActionBindItem>();
        if (item.IsInitHandlerCalled == false)
        {
            item.IsInitHandlerCalled = true;
        }
        itemScript.SetData(data, index);
        return item;
    }

    public void AddActionBind(ActionTriggerData data)
    {
        ActionTriggerDatas.datas.Add(data);
        ES3.Save<ActionTriggerDatas>("ActionTriggerDatas", ActionTriggerDatas);
        ListView.SetListItemCount(ActionTriggerDatas.datas.Count);
    }

    public void DeleteActionBind(int index)
    {
        ActionTriggerDatas.datas.RemoveAt(index);
        ListView.SetListItemCount(ActionTriggerDatas.datas.Count);
    }
}
