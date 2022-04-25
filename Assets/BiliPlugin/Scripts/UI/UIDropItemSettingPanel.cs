using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDropItemSettingPanel : MonoBehaviour
{
    public TextMeshProUGUI GiftNameText;
    public Image GiftImage;
    public TMP_InputField LifeTimeInput;
    public TMP_InputField ScaleInput;
    public TMP_InputField ColliderRadiusInput;
    public TMP_InputField PerTriggerDropCountInput;
    public Button SaveBtn;
    public Button CloseBtn;

    private string GiftName;

    private void Awake()
    {
        SaveBtn.onClick.AddListener(OnClickSaveBtn);
    }

    void OnClickSaveBtn()
    {
        var data = BiliPlugin.Instance.DropItemManager.DropItemDataDict[GiftName];
        data.LifeTime = float.Parse(LifeTimeInput.text);
        data.Scale = float.Parse(ScaleInput.text);
        data.ColliderRadius = float.Parse(ColliderRadiusInput.text);
        data.PerTriggerDropCount = int.Parse(PerTriggerDropCountInput.text);
        BiliPlugin.Instance.DropItemManager.SaveItem(GiftName, data);
        gameObject.SetActive(false);
        BiliPlugin.Instance.UIDropItemPanel.Refresh();
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
    }
}
