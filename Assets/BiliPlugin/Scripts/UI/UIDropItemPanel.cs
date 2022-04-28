using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SuperScrollView;

public class UIDropItemPanel : MonoBehaviour
{
    public Button RefreshBtn;
    public Button AllEditBtn;
    public LoopListView2 ListView;
    public Toggle EnableModelColliderToggle;
    public TMP_InputField ColliderRadiusInput;
    public TMP_InputField ColliderOffsetXInput;
    public TMP_InputField ColliderOffsetYInput;
    public List<string> GiftNameList = new List<string>();

    private void Awake()
    {
        RefreshBtn.onClick.AddListener(Refresh);
        AllEditBtn.onClick.AddListener(OnClickAllEditBtn);
        ListView.InitListView(0, OnGetItemByIndex);

        var data = BiliPlugin.Instance.DropItemManager.SetModelColliderData;
        EnableModelColliderToggle.isOn = data.Enable;
        ColliderRadiusInput.text = data.Radius.ToString();
        ColliderOffsetXInput.text = data.OffsetX.ToString();
        ColliderOffsetYInput.text = data.OffsetY.ToString();

        EnableModelColliderToggle.onValueChanged.AddListener((v)=>SaveAndSendColliderData());
        ColliderRadiusInput.onEndEdit.AddListener((v) => SaveAndSendColliderData());
        ColliderOffsetXInput.onEndEdit.AddListener((v) => SaveAndSendColliderData());
        ColliderOffsetYInput.onEndEdit.AddListener((v) => SaveAndSendColliderData());
    }

    void Start()
    {
        Refresh();
    }

    public void SaveAndSendColliderData()
    {
        VTSSetModelColliderData.Data data = new VTSSetModelColliderData.Data();
        data.Enable = EnableModelColliderToggle.isOn;
        float.TryParse(ColliderRadiusInput.text, out data.Radius);
        float.TryParse(ColliderOffsetXInput.text, out data.OffsetX);
        float.TryParse(ColliderOffsetYInput.text, out data.OffsetY);
        BiliPlugin.Instance.DropItemManager.SetModelColliderData = data;
        BiliPlugin.Instance.DropItemManager.SaveAndSendCollider();
    }

    public void OnClickAllEditBtn()
    {
        BiliPlugin.Instance.UIDropItemSettingPanel.SetAllGiftEditMode(true);
        BiliPlugin.Instance.UIDropItemSettingPanel.gameObject.SetActive(true);
    }

    public void Refresh()
    {
        BiliPlugin.Instance.DropItemManager.ReloadDropItemData();
        GiftNameList.Clear();
        foreach (var kv in BiliPlugin.Instance.DropItemManager.DropItemDataDict)
        {
            GiftNameList.Add(kv.Key);
        }
        ListView.SetListItemCount(GiftNameList.Count);
        ListView.RefreshAllShownItem();
        BiliPlugin.Instance.SendRefreshDropItem((s) => { }, (e) => { });
        var collider = BiliPlugin.Instance.DropItemManager.SetModelColliderData;
        EnableModelColliderToggle.isOn = collider.Enable;
        ColliderRadiusInput.text = collider.Radius.ToString();
        ColliderOffsetXInput.text = collider.OffsetX.ToString();
        ColliderOffsetYInput.text = collider.OffsetY.ToString();
    }

    LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (index < 0 || index >= GiftNameList.Count)
        {
            return null;
        }

        string giftName = GiftNameList[index];
        if (string.IsNullOrWhiteSpace(giftName))
        {
            return null;
        }
        LoopListViewItem2 item = listView.NewListViewItem("DropItemPreviewPrefab");
        UIDropItemPreviewItem itemScript = item.GetComponent<UIDropItemPreviewItem>();
        if (item.IsInitHandlerCalled == false)
        {
            item.IsInitHandlerCalled = true;
        }
        itemScript.SetData(giftName, index);
        return item;
    }
}
