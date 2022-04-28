using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICreateActionBindPanel : MonoBehaviour
{
    public Button CloseBtn;
    public Button FinishCreateBtn;
    public TMP_Dropdown SelectTriggerTypeDP;
    public GameObject GiftNameTitle;
    public TMP_InputField GiftNameInputField;
    public GameObject MinBatteryTitle;
    public TMP_InputField MinBatteryInputField;
    public GameObject MaxBatteryTitle;
    public TMP_InputField MaxBatteryInputField;
    public TMP_InputField TriggerCDInputField;
    public TMP_Dropdown SelectActionDP;
    public TMP_InputField ActionTimeInputField;

    void Start()
    {
        CloseBtn.onClick.AddListener(OnClickCloseBtn);
        FinishCreateBtn.onClick.AddListener(OnClickFinishCreateBtn);
        SelectTriggerTypeDP.onValueChanged.AddListener(OnSelectTriggerTypeDPValueChanged);
        OnSelectTriggerTypeDPValueChanged(0);
        SelectActionDP.ClearOptions();
        SelectActionDP.AddOptions(new List<string>() { "无动作" });
        foreach (var hotkey in BiliPlugin.Instance.UIActionPanel.HotkeyDataList)
        {
            SelectActionDP.AddOptions(new List<string>() { hotkey.name });
        }
    }

    private void Update()
    {
        FinishCreateBtn.interactable = CanFinishCreate();
    }

    private void OnClickCloseBtn()
    {
        Destroy(gameObject);
    }

    private bool CanFinishCreate()
    {
        bool isCanFinish = true;
        float cd;
        if (!float.TryParse(TriggerCDInputField.text, out cd))
            isCanFinish = false;
        if (cd < 0)
            isCanFinish = false;
        ActionTriggerType TriggerType = (ActionTriggerType)SelectTriggerTypeDP.value;
        switch (TriggerType)
        {
            case ActionTriggerType.收到特定礼物时触发:
                if (string.IsNullOrWhiteSpace(GiftNameInputField.text))
                    isCanFinish = false;
                break;
            case ActionTriggerType.收到礼物金额满足条件时触发:
                int min, max;
                if (!int.TryParse(MinBatteryInputField.text, out min))
                    isCanFinish = false;
                if (!int.TryParse(MaxBatteryInputField.text, out max))
                    isCanFinish = false;
                if (min > max)
                    isCanFinish = false;
                break;
            case ActionTriggerType.收到SC时触发:
                break;
            case ActionTriggerType.收到舰长时触发:
                break;
        }
        if (SelectActionDP.value == 0)
            isCanFinish = false;
        return isCanFinish;
    }

    private void OnClickFinishCreateBtn()
    {
        if (CanFinishCreate())
        {
            ActionTriggerData TriggerData = new ActionTriggerData();
            TriggerData.TriggerType = (ActionTriggerType)SelectTriggerTypeDP.value;
            TriggerData.GiftName = GiftNameInputField.text;
            TriggerData.ActionName = SelectActionDP.options[SelectActionDP.value].text;
            int.TryParse(MinBatteryInputField.text, out TriggerData.MinBattery);
            int.TryParse(MaxBatteryInputField.text, out TriggerData.MaxBattery);
            float.TryParse(TriggerCDInputField.text, out TriggerData.TriggerCD);
            float.TryParse(ActionTimeInputField.text, out TriggerData.ActionTime);
            BiliPlugin.Instance.UIActionPanel.AddActionBind(TriggerData);
            OnClickCloseBtn();
        }
    }

    private void OnSelectTriggerTypeDPValueChanged(int value)
    {
        GiftNameTitle.SetActive(false);
        GiftNameInputField.gameObject.SetActive(false);
        MinBatteryTitle.SetActive(false);
        MinBatteryInputField.gameObject.SetActive(false);
        MaxBatteryTitle.SetActive(false);
        MaxBatteryInputField.gameObject.SetActive(false);
        ActionTriggerType TriggerType = (ActionTriggerType)value;
        switch (TriggerType)
        {
            case ActionTriggerType.收到特定礼物时触发:
                GiftNameTitle.SetActive(true);
                GiftNameInputField.gameObject.SetActive(true);
                break;
            case ActionTriggerType.收到礼物金额满足条件时触发:
                MinBatteryTitle.SetActive(true);
                MinBatteryInputField.gameObject.SetActive(true);
                MaxBatteryTitle.SetActive(true);
                MaxBatteryInputField.gameObject.SetActive(true);
                break;
            case ActionTriggerType.收到SC时触发:
                break;
            case ActionTriggerType.收到舰长时触发:
                break;
        }
    }
}
