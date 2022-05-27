// By 宵夜97
using System.Collections;
using System.Collections.Generic;
using Lean.Gui;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VTS_XYPlugin_Common;

public class UIActionBindItem : MonoBehaviour
{
    public Text DescText;
    public LeanToggle Enable;
    private GiftTriggerActionData Data;

    private void Start()
    {
        Enable.OnOn.AddListener(() =>
        {
            Data.Enable = true;
            UIPageActionBind.Instance.SaveConfig();
        });
        Enable.OnOff.AddListener(() =>
        {
            Data.Enable = false;
            UIPageActionBind.Instance.SaveConfig();
        });
    }

    private void OnDisable()
    {
        Enable.OnOn.RemoveAllListeners();
        Enable.OnOff.RemoveAllListeners();
    }

    public void SetData(GiftTriggerActionData data)
    {
        Data = data;
        Enable.On = data.Enable;
        string desc = "";
        switch (Data.GiftTriggerActionType)
        {
            case GiftTriggerActionType.收到特定礼物时触发:
                desc = $"当收到礼物<color=magenta>{Data.TriggerGiftName}</color>时，触发动作<color=magenta>{Data.ActionName}</color>。";
                break;
            case GiftTriggerActionType.收到礼物满足条件时触发:
                desc = $"当收到了礼物金额满足 <color=magenta>{Data.MinMoneyLimit}电池≤礼物金额≤{Data.MaxMoneyLimit}电池</color> 时，触发动作<color=magenta>{Data.ActionName}</color>。";
                break;
            case GiftTriggerActionType.收到SC时触发:
                desc = $"当收到SC时，触发动作<color=magenta>{Data.ActionName}</color>。";
                break;
            case GiftTriggerActionType.收到舰长时触发:
                desc = $"当收到<color=magenta>{Data.JianZhangType}</color>时，触发动作<color=magenta>{Data.ActionName}</color>。";
                break;
        }
        DescText.text = desc;
    }

    public void Edit()
    {
        UIWindowCreateBind.Instance.SetData(Data);
        UIWindowCreateBind.Instance.Open();
    }

    public void Delete()
    {
        XYPlugin.Instance.NowModelConfig.TriggerActionData.Remove(Data);
        UIPageActionBind.Instance.SaveConfig();
        UIPageActionBind.Instance.Refresh();
    }
}
