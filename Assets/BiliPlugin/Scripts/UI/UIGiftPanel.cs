using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SuperScrollView;
using TMPro;
using Sirenix.OdinInspector;

public class UIGiftPanel : MonoBehaviour
{
    public LoopListView2 ListView;
    public TMP_InputField CountLimitInput;
    public TMP_InputField MoneyLimitInput;
    public Toggle OnlyShowActionToggle;
    public Button ClearBtn;
    private List<GiftData> giftDatas;

    public int GiftCountLimit;
    public int GiftMoneyLimit;
    public bool OnlyShowAction;


    public int LogCount
    {
        get
        {
            return giftDatas.Count;
        }
    }

    void Start()
    {
        giftDatas = new List<GiftData>();
        ListView.InitListView(LogCount, OnGetItemByIndex);
        GiftCountLimit = ES3.Load<int>("GiftCountLimit", 100);
        CountLimitInput.text = GiftCountLimit.ToString();
        CountLimitInput.onEndEdit.AddListener(OnEndEditCountLimitInput);
        SetCountLimit(GiftCountLimit);
        GiftMoneyLimit = ES3.Load<int>("GiftMoneyLimit", 0);
        MoneyLimitInput.text = GiftMoneyLimit.ToString();
        MoneyLimitInput.onEndEdit.AddListener(OnEndEditMoneyLimitInput);
        OnlyShowAction = ES3.Load<bool>("GiftOnlyShowAction", false);
        OnlyShowActionToggle.isOn = OnlyShowAction;
        OnlyShowActionToggle.onValueChanged.AddListener(OnValueChangedOnlyShowActionToggle);
        ClearBtn.onClick.AddListener(OnClickClearBtn);
    }

    private void OnEndEditCountLimitInput(string input)
    {
        int limit = 1;
        int.TryParse(input, out limit);
        Mathf.Max(1, limit);
        SetCountLimit(limit);
        ES3.Save<int>("GiftCountLimit", limit);
    }

    private void OnEndEditMoneyLimitInput(string input)
    {
        int limit = 0;
        int.TryParse(input, out limit);
        Mathf.Max(0, limit);
        GiftMoneyLimit = limit;
        ES3.Save<int>("GiftMoneyLimit", limit);
    }

    private void OnValueChangedOnlyShowActionToggle(bool value)
    {
        OnlyShowAction = value;
        ES3.Save<bool>("GiftOnlyShowAction", value);
    }

    private void OnClickClearBtn()
    {
        giftDatas.Clear();
        ListView.SetListItemCount(0);
    }

    [Button]
    public void OnGift(GiftData gift)
    {
        // 此处UI中是电池，数据中是人民币，1人民币=10电池
        // 过滤电池限制
        if (GiftMoneyLimit > 0)
        {
            if (gift.GiftType == GiftType.Gift || gift.GiftType == GiftType.SC)
            {
                if (gift.Money * 10 < GiftMoneyLimit)
                {
                    return;
                }
            }
        }
        // 过滤动作限制
        if (OnlyShowAction)
        {
            if (string.IsNullOrWhiteSpace(gift.ActionName))
            {
                return;
            }
        }
        giftDatas.Add(gift);
        if (giftDatas.Count > GiftCountLimit)
        {
            giftDatas.RemoveAt(0);
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
        GiftCountLimit = limit;
        if (giftDatas.Count > limit)
        {
            int count = giftDatas.Count - limit;
            for (int i = 0; i < count; i++)
            {
                giftDatas.RemoveAt(0);
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

        GiftData giftData = giftDatas[index];
        if (giftData == null)
        {
            return null;
        }
        LoopListViewItem2 item = listView.NewListViewItem("GiftPrefab");
        UIGiftItem itemScript = item.GetComponent<UIGiftItem>();
        if (item.IsInitHandlerCalled == false)
        {
            item.IsInitHandlerCalled = true;
        }
        itemScript.SetData(giftData, index);
        return item;
    }
}
