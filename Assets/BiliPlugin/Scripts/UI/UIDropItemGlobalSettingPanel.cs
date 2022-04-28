using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDropItemGlobalSettingPanel : MonoBehaviour
{
    public TMP_InputField DropPosXInput;
    public TMP_InputField DropPosYInput;
    public TMP_InputField DropBaseSpeedInput;
    public TMP_InputField DropHighSpeedInput;
    public TMP_InputField DropChangeSpeedInput;
    public Button SaveBtn;

    private void Awake()
    {
        SaveBtn.onClick.AddListener(OnClickSaveBtn);
    }

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        var data = BiliPlugin.Instance.DropItemManager.DropSettingData;
        DropPosXInput.text = data.DropPosX.ToString();
        DropPosYInput.text = data.DropPosY.ToString();
        DropBaseSpeedInput.text = data.BaseSpeed.ToString();
        DropHighSpeedInput.text = data.HighSpeed.ToString();
        DropChangeSpeedInput.text = data.ChangeSpeedCount.ToString();
    }

    public void OnClickSaveBtn()
    {
        var data = BiliPlugin.Instance.DropItemManager.DropSettingData;
        float.TryParse(DropPosXInput.text, out data.DropPosX);
        float.TryParse(DropPosYInput.text, out data.DropPosY);
        float.TryParse(DropBaseSpeedInput.text, out data.BaseSpeed);
        float.TryParse(DropHighSpeedInput.text, out data.HighSpeed);
        int.TryParse(DropChangeSpeedInput.text, out data.ChangeSpeedCount);
        BiliPlugin.Instance.DropItemManager.SaveDropSetting();
        gameObject.SetActive(false);
    }
}
