using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDropItemPreviewItem : MonoBehaviour
{
    public Image GiftImage;
    public Button EditBtn;
    public Button Test1Btn;
    public Button Test10Btn;
    public Button Test100Btn;
    public TextMeshProUGUI InfoText;
    private string GiftName;

    public void SetData(string giftName, int index)
    {
        GiftName = giftName;
        GiftImage.sprite = BiliPlugin.Instance.DropItemManager.SpriteDict[giftName];
        var data = BiliPlugin.Instance.DropItemManager.DropItemDataDict[giftName];
        string info = "";
        info += $"礼物名字:{giftName}\n";
        info += $"存活时间:{data.LifeTime}s\n";
        info += $"图片缩放:{data.Scale}\n";
        info += $"碰撞半径:{data.ColliderRadius}\n";
        info += $"每次触发掉落数量:{data.PerTriggerDropCount}\n";
        InfoText.text = info;
    }

    public void OnClickEditBtn()
    {
        BiliPlugin.Instance.UIDropItemSettingPanel.SetData(GiftName);
        BiliPlugin.Instance.UIDropItemSettingPanel.gameObject.SetActive(true);
    }

    public void OnClickTestBtn(int count)
    {
        BiliPlugin.Instance.DropItemManager.DropItem(GiftName, count);
    }
}
