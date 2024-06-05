// By 宵夜97
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Gui;
using VTS_XYPlugin_Common;
using System;
using System.Linq;
using System.Diagnostics;

public class UIPageConfig : MonoSingleton<UIPageConfig>
{
    public LeanToggle AutoOpenGUIToggle;
    public LeanToggle DebugModeToggle;
    public LeanToggle HideItemOnAlphaArtMeshToggle;
    public LeanToggle DownloadHeadToggle;

    private float checkSaveCD = 1f;

    public override void Awake()
    {
        base.Awake();
        var keys = Enum.GetNames(typeof(RawKeyMap)).ToList();
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
        var config = XYPlugin.Instance.GlobalConfig;
        if (AutoOpenGUIToggle.On != config.AutoOpenGUI) return true;
        if (DebugModeToggle.On != config.DebugMode) return true;
        if (HideItemOnAlphaArtMeshToggle.On != config.HideItemOnAlphaArtMesh) return true;
        return false;
    }

    public void Refresh()
    {
        var config = XYPlugin.Instance.GlobalConfig;
        AutoOpenGUIToggle.On = config.AutoOpenGUI;
        DebugModeToggle.On = config.DebugMode;
        HideItemOnAlphaArtMeshToggle.On = config.HideItemOnAlphaArtMesh;
        DownloadHeadToggle.On = config.DownloadHeadOnRecvGift;
    }

    public void SaveConfig()
    {
        var config = XYPlugin.Instance.GlobalConfig;
        config.AutoOpenGUI = AutoOpenGUIToggle.On;
        config.DebugMode = DebugModeToggle.On;
        config.HideItemOnAlphaArtMesh = HideItemOnAlphaArtMeshToggle.On;
        config.DownloadHeadOnRecvGift = DownloadHeadToggle.On;
        FileHelper.SaveGlobalConfig();
    }
    
    public void HowToConnectBilibili()
    {
        Process.Start($"{Application.streamingAssetsPath}/连接B站弹幕机教程.pdf");
    }
}