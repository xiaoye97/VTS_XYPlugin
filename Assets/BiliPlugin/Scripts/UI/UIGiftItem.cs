using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGiftItem : MonoBehaviour
{
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI GiftText;

    public void SetData(GiftData data, int index)
    {
        TimeText.text = data.Time.ToString();
        string giftText = "";
        switch (data.GiftType)
        {
            case GiftType.Gift:
                giftText = $"{data.UserName}赠送了{data.GiftName}x{data.GiftCount}";
                if (data.Money > 0)
                {
                    giftText += $" (￥{data.Money})";
                }
                break;
            case GiftType.SC:
                giftText = $"{data.UserName}发送了SC(￥{data.Money}):{data.GiftMsg}";
                break;
            case GiftType.Jian:
                giftText = $"{data.UserName}开通了{data.GiftName}";
                break;
        }
        if (!string.IsNullOrWhiteSpace(data.ActionName))
        {
            giftText += $"\n触发了动作:{data.ActionName}";
        }
        GiftText.text = giftText;
    }
}
