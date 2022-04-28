using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDropItemSettingPanel : MonoBehaviour
{
    public Sprite AllGift;
    public TextMeshProUGUI GiftNameText;
    public Image GiftImage;
    public TMP_InputField LifeTimeInput;
    public TMP_InputField ScaleInput;
    public TMP_InputField ColliderRadiusInput;
    public TMP_InputField PerTriggerDropCountInput;
    public TMP_InputField OrderInput;
    public Button SaveBtn;
    public Button CloseBtn;

    private string GiftName;
    private bool allGiftEdotMode;

    private void Awake()
    {
        SaveBtn.onClick.AddListener(OnClickSaveBtn);
    }

    void OnClickSaveBtn()
    {
        if (allGiftEdotMode)
        {
            SaveAllGiftData();
        }
        else
        {
            SaveNowGiftData();
        }
        gameObject.SetActive(false);
        BiliPlugin.Instance.UIDropItemPanel.Refresh();
    }

    public void SaveAllGiftData()
    {
        float lifeTime, radius, scale;
        int perTriggerDropCount, order;
        lifeTime = float.Parse(LifeTimeInput.text);
        scale = float.Parse(ScaleInput.text);
        radius = float.Parse(ColliderRadiusInput.text);
        perTriggerDropCount = int.Parse(PerTriggerDropCountInput.text);
        order = int.Parse(OrderInput.text);
        List<string> keys = new List<string>();
        foreach(var kv in BiliPlugin.Instance.DropItemManager.DropItemDataDict)
        {
            keys.Add(kv.Key);
        }
        for(int i = 0; i < keys.Count; i++)
        {
            string giftName = keys[i];
            var data = BiliPlugin.Instance.DropItemManager.DropItemDataDict[giftName];
            data.LifeTime = lifeTime;
            data.Scale = scale;
            data.ColliderRadius = radius;
            data.PerTriggerDropCount = perTriggerDropCount;
            data.Order = order;
            BiliPlugin.Instance.DropItemManager.SaveItem(giftName, data);
        }
    }

    public void SaveNowGiftData()
    {
        var data = BiliPlugin.Instance.DropItemManager.DropItemDataDict[GiftName];
        data.LifeTime = float.Parse(LifeTimeInput.text);
        data.Scale = float.Parse(ScaleInput.text);
        data.ColliderRadius = float.Parse(ColliderRadiusInput.text);
        data.PerTriggerDropCount = int.Parse(PerTriggerDropCountInput.text);
        data.Order = int.Parse(OrderInput.text);
        BiliPlugin.Instance.DropItemManager.SaveItem(GiftName, data);
    }

    public void SetAllGiftEditMode(bool onoff)
    {
        allGiftEdotMode = onoff;
        if (onoff)
        {
            GiftName = "全部礼物";
            GiftImage.sprite = AllGift;
            GiftNameText.text = "全部礼物";
            LifeTimeInput.text = "";
            ScaleInput.text = "";
            ColliderRadiusInput.text = "";
            PerTriggerDropCountInput.text = "";
            OrderInput.text = "";
        }
    }

    public void SetData(string giftName)
    {
        GiftName = giftName;
        GiftImage.sprite = BiliPlugin.Instance.DropItemManager.SpriteDict[giftName];
        var data = BiliPlugin.Instance.DropItemManager.DropItemDataDict[giftName];
        GiftNameText.text = giftName;
        LifeTimeInput.text = data.LifeTime.ToString();
        ScaleInput.text = data.Scale.ToString();
        ColliderRadiusInput.text = data.ColliderRadius.ToString();
        PerTriggerDropCountInput.text = data.PerTriggerDropCount.ToString();
        OrderInput.text = data.Order.ToString();
    }
}
