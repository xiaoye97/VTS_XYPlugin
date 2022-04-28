using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDropItemMiniPanel : MonoBehaviour
{
    public Button OpenDirBtn;

    private void Awake()
    {
        OpenDirBtn.onClick.AddListener(OnClickOpenDirBtn);
    }

    public void OnClickOpenDirBtn()
    {
        System.Diagnostics.Process.Start(BiliPlugin.Instance.DropItemManager.ImageDirectory.FullName);
    }
}
