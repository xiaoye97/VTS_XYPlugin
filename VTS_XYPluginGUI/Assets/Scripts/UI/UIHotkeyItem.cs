// By 宵夜97
using System.Collections;
using System.Collections.Generic;
using Lean.Gui;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VTS_XYPlugin_Common;

public class UIHotkeyItem : MonoBehaviour
{
    public Text HotkeyText;
    private string Hotkey;

    public void SetData(string hotkey)
    {
        Hotkey = hotkey;
        HotkeyText.text = Hotkey;
    }

    public void CreateBind()
    {
        GiftTriggerActionData data = new GiftTriggerActionData();
        data.ActionName = Hotkey;
        UIWindowCreateBind.Instance.SetData(data);
        UIWindowCreateBind.Instance.Open();
    }

    public void TriggerAction()
    {
        XYGUICache cache = new XYGUICache();
        cache.TestTriggerHotkeys.Add(Hotkey);
        XYNetClient.Instance.SendCache(cache);
    }
}
