// By 宵夜97
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Gui;
using VTS_XYPlugin_Common;
using System;
using System.Linq;

public class UIPageConfig : MonoSingleton<UIPageConfig>
{
    public InputField RoomIDInput;
    public LeanToggle AutoOpenGUIToggle;
    public LeanToggle DebugModeToggle;
    public LeanToggle HideItemOnAlphaArtMeshToggle;
    public LeanToggle DownloadHeadToggle;
    public Dropdown SwitchMessageSystemHotkey;
    public Dropdown SwitchDropGiftHotkey;
    public Dropdown SwitchTriggerActionHotkey;

    private float checkSaveCD = 1f;

    public override void Awake()
    {
        base.Awake();
        SwitchMessageSystemHotkey.ClearOptions();
        SwitchDropGiftHotkey.ClearOptions();
        SwitchTriggerActionHotkey.ClearOptions();
        var keys = Enum.GetNames(typeof(RawKeyMap)).ToList();
        SwitchMessageSystemHotkey.AddOptions(keys);
        SwitchDropGiftHotkey.AddOptions(keys);
        SwitchTriggerActionHotkey.AddOptions(keys);
    }

    private void Update()
    {
        checkSaveCD -= Time.deltaTime;
        if (checkSaveCD < 0)
        {
            checkSaveCD = 1f;
            if (IsNeedSave())
            {
                SaveConfig();
            }
        }
    }

    public bool IsNeedSave()
    {
        if (RoomIDInput.isFocused) return false;
        var config = XYPlugin.Instance.GlobalConfig;
        if (AutoOpenGUIToggle.On != config.AutoOpenGUI) return true;
        if (DebugModeToggle.On != config.DebugMode) return true;
        if (HideItemOnAlphaArtMeshToggle.On != config.HideItemOnAlphaArtMesh) return true;
        if (RoomIDInput.text != config.BLiveConfig.RoomID.ToString()) return true;
        if (SwitchMessageSystemHotkey.GetText() != config.SwitchMessageSystemHotkey.ToString()) return true;
        if (SwitchDropGiftHotkey.GetText() != config.SwitchDropGiftHotkey.ToString()) return true;
        if (SwitchTriggerActionHotkey.GetText() != config.SwitchTriggerActionHotkey.ToString()) return true;
        return false;
    }

    public void Refresh()
    {
        var config = XYPlugin.Instance.GlobalConfig;
        RoomIDInput.text = config.BLiveConfig.RoomID.ToString();
        AutoOpenGUIToggle.On = config.AutoOpenGUI;
        DebugModeToggle.On = config.DebugMode;
        HideItemOnAlphaArtMeshToggle.On = config.HideItemOnAlphaArtMesh;
        DownloadHeadToggle.On = config.DownloadHeadOnRecvGift;
        SwitchMessageSystemHotkey.SetValue(config.SwitchMessageSystemHotkey.ToString());
        SwitchDropGiftHotkey.SetValue(config.SwitchDropGiftHotkey.ToString());
        SwitchTriggerActionHotkey.SetValue(config.SwitchTriggerActionHotkey.ToString());
    }

    public void SaveConfig()
    {
        var config = XYPlugin.Instance.GlobalConfig;
        int.TryParse(RoomIDInput.text, out config.BLiveConfig.RoomID);
        config.AutoOpenGUI = AutoOpenGUIToggle.On;
        config.DebugMode = DebugModeToggle.On;
        config.HideItemOnAlphaArtMesh = HideItemOnAlphaArtMeshToggle.On;
        config.DownloadHeadOnRecvGift = DownloadHeadToggle.On;
        config.SwitchMessageSystemHotkey = SwitchMessageSystemHotkey.GetEnum<RawKeyMap>();
        config.SwitchDropGiftHotkey = SwitchDropGiftHotkey.GetEnum<RawKeyMap>();
        config.SwitchTriggerActionHotkey = SwitchTriggerActionHotkey.GetEnum<RawKeyMap>();
        FileHelper.SaveGlobalConfig();
    }
}
