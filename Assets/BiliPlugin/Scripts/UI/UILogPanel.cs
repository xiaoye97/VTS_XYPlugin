using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SuperScrollView;
using TMPro;
using Sirenix.OdinInspector;

public class UILogPanel : MonoBehaviour
{
    public LoopListView2 ListView;
    public TMP_InputField LimitInput;
    public Button ClearBtn;
    private List<LogData> logDatas;
    private int logCountLimit;

    public int LogCount
    {
        get
        {
            return logDatas.Count;
        }
    }

    void Start()
    {
        logDatas = new List<LogData>();
        ListView.InitListView(LogCount, OnGetItemByIndex);
        int limit = ES3.Load<int>("LogCountLimit", 100);
        LimitInput.text = limit.ToString();
        LimitInput.onEndEdit.AddListener(OnEndEditLimitInput);
        SetCountLimit(limit);
        ClearBtn.onClick.AddListener(OnClickClearBtn);
    }

    private void OnEndEditLimitInput(string input)
    {
        int limit = 1;
        int.TryParse(input, out limit);
        Mathf.Max(1, limit);
        SetCountLimit(limit);
        ES3.Save<int>("LogCountLimit", limit);
    }

    private void OnClickClearBtn()
    {
        logDatas.Clear();
        ListView.SetListItemCount(0);
    }

    [Button]
    public void Log(string msg, LogType logType = LogType.Log)
    {
        logDatas.Add(new LogData(msg, logType));
        if (logDatas.Count > logCountLimit)
        {
            logDatas.RemoveAt(0);
        }
        ListView.SetListItemCount(LogCount);
        if (LogCount > 10)
        {
            ListView.MovePanelToItemIndex(LogCount - 1, 0);
        }
    }

    /// <summary>
    /// 设置显示数量限制
    /// </summary>
    /// <param name="limit"></param>
    public void SetCountLimit(int limit)
    {
        logCountLimit = limit;
        if (logDatas.Count > limit)
        {
            int count = logDatas.Count - limit;
            for (int i = 0; i < count; i++)
            {
                logDatas.RemoveAt(0);
            }
        }
        ListView.SetListItemCount(LogCount);
    }

    LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (index < 0 || index >= LogCount)
        {
            return null;
        }

        LogData logData = logDatas[index];
        if (logData == null)
        {
            return null;
        }
        LoopListViewItem2 item = listView.NewListViewItem("LogPrefab");
        UILogItem itemScript = item.GetComponent<UILogItem>();
        if (item.IsInitHandlerCalled == false)
        {
            item.IsInitHandlerCalled = true;
        }
        itemScript.SetData(logData, index);
        return item;
    }
}
