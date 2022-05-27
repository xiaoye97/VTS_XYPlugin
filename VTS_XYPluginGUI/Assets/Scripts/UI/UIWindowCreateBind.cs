// By 宵夜97
using Lean.Gui;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VTS_XYPlugin_Common;
using System.Collections.Generic;

public class UIWindowCreateBind : MonoSingleton<UIWindowCreateBind>
{
    public LeanWindow Window;
    public RectTransform ContentRT;
    public LeanButton OkBtn;
    public CanvasGroup OkBtnGroup;
    public LeanButton CancelBtn;
    public InputField ActionName;
    public Dropdown GiftTriggerActionTypeDP;
    public InputField TriggerGiftName;
    public InputField MinMoneyLimit;
    public InputField MaxMoneyLimit;
    public Dropdown JianZhangType;
    public InputField TriggerCD;
    public LeanToggle Enable;
    private GiftTriggerActionData Data;

    public override void Awake()
    {
        base.Awake();
        Init();
    }

    private void Update()
    {
        if (Window.On)
        {
            bool canFinish = CanFinish();
            OkBtn.interactable = canFinish;
            if (canFinish) OkBtnGroup.alpha = 1.0f;
            else OkBtnGroup.alpha = 0.6f;
        }
    }

    private void Init()
    {
        GiftTriggerActionTypeDP.onValueChanged.AddListener((v) =>
        {
            OnSelectGiftTriggerActionType((GiftTriggerActionType)v);
        });
    }

    public void OnSelectGiftTriggerActionType(GiftTriggerActionType type)
    {
        TriggerGiftName.gameObject.SetActive(false);
        MinMoneyLimit.gameObject.SetActive(false);
        MaxMoneyLimit.gameObject.SetActive(false);
        JianZhangType.gameObject.SetActive(false);
        switch (type)
        {
            case GiftTriggerActionType.收到特定礼物时触发:
                TriggerGiftName.gameObject.SetActive(true);
                break;
            case GiftTriggerActionType.收到礼物满足条件时触发:
                MinMoneyLimit.gameObject.SetActive(true);
                MaxMoneyLimit.gameObject.SetActive(true);
                break;
            case GiftTriggerActionType.收到SC时触发:
                break;
            case GiftTriggerActionType.收到舰长时触发:
                JianZhangType.gameObject.SetActive(true);
                break;
        }
    }

    public void SetData(GiftTriggerActionData data)
    {
        Data = data;
        ActionName.text = data.ActionName;
        GiftTriggerActionTypeDP.value = (int)data.GiftTriggerActionType;
        OnSelectGiftTriggerActionType(data.GiftTriggerActionType);
        TriggerGiftName.text = data.TriggerGiftName;
        MinMoneyLimit.text = data.MinMoneyLimit.ToString();
        MaxMoneyLimit.text = data.MaxMoneyLimit.ToString();
        JianZhangType.value = (int)data.JianZhangType - 1;
        TriggerCD.text = data.TriggerCD.ToString();
        Enable.On = data.Enable;
    }

    public void ToTop()
    {
        ContentRT.anchoredPosition = new Vector2(ContentRT.anchoredPosition.x, 0);
    }

    public bool CanFinish()
    {
        if (string.IsNullOrWhiteSpace(ActionName.text)) return false;
        if (string.IsNullOrWhiteSpace(TriggerCD.text)) return false;
        switch ((GiftTriggerActionType)GiftTriggerActionTypeDP.value)
        {
            case GiftTriggerActionType.收到特定礼物时触发:
                if (string.IsNullOrWhiteSpace(TriggerGiftName.text)) return false;
                break;
            case GiftTriggerActionType.收到礼物满足条件时触发:
                if (string.IsNullOrWhiteSpace(MinMoneyLimit.text)) return false;
                if (string.IsNullOrWhiteSpace(MaxMoneyLimit.text)) return false;
                break;
        }
        return true;
    }

    public void FinishCreate()
    {
        if (!CanFinish()) return;
        Data.GiftTriggerActionType = (GiftTriggerActionType)GiftTriggerActionTypeDP.value;
        float.TryParse(TriggerCD.text, out Data.TriggerCD);
        Data.TriggerGiftName = TriggerGiftName.text;
        int.TryParse(MinMoneyLimit.text, out Data.MinMoneyLimit);
        int.TryParse(MaxMoneyLimit.text, out Data.MaxMoneyLimit);
        Data.JianZhangType = (BJianDuiType)(JianZhangType.value + 1);
        Data.Enable = Enable.On;
        // 如果不在列表中，则加入到列表
        if (!XYPlugin.Instance.NowModelConfig.TriggerActionData.Contains(Data))
        {
            XYPlugin.Instance.NowModelConfig.TriggerActionData.Add(Data);
        }
        Close();
        UIPageActionBind.Instance.SaveConfig();
        UIPageActionBind.Instance.Refresh();
    }

    public void Open()
    {
        ToTop();
        Window.TurnOn();
    }

    public void Close()
    {
        Window.TurnOff();
    }
}
