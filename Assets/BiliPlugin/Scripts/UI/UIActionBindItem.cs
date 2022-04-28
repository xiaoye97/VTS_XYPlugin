using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIActionBindItem : MonoBehaviour
{
    public TextMeshProUGUI ActionNameText;
    public TextMeshProUGUI ActionDescriptionText;
    public Button DeleteBtn;

    private int itemIndex;

    public void SetData(ActionTriggerData data, int index)
    {
        itemIndex = index;
        ActionNameText.text = $"动作:{data.ActionName} CD:{data.TriggerCD} 动作时长:{data.ActionTime}";
        string desc = "";
        desc += $"类型:{data.TriggerType}";
        switch (data.TriggerType)
        {
            case ActionTriggerType.收到特定礼物时触发:
                desc += $" {data.GiftName}";
                break;
            case ActionTriggerType.收到礼物金额满足条件时触发:
                desc += $" {data.MinBattery}电池-{data.MaxBattery}电池";
                break;
        }
        ActionDescriptionText.text = desc;
        DeleteBtn.onClick.RemoveAllListeners();
        DeleteBtn.onClick.AddListener(OnClickDeleteBtn);
    }

    private void OnClickDeleteBtn()
    {
        BiliPlugin.Instance.UIActionPanel.DeleteActionBind(itemIndex);
    }
}
