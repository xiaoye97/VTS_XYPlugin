// By 宵夜97
using System;
using Lean.Gui;
using System.IO;
using Lean.Pool;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VTS_XYPlugin_Common;
using System.Collections.Generic;

public class UIPageActionBind : MonoSingleton<UIPageActionBind>
{
    public GameObject HasModel;
    public GameObject NoModel;
    public LeanGameObjectPool BindPool;
    public RectTransform BindRT;
    public LeanGameObjectPool ActionPool;
    public RectTransform ActionRT;

    void Start()
    {
        Refresh();
    }

    public void Refresh(bool loadConfig = false)
    {
        if (UIWindowCreateBind.Instance.Window.On)
        {
            UIWindowCreateBind.Instance.Close();
            UINotification.Instance.Show("模型配置文件已经更改，取消创建绑定");
        }
        bool hasModel = !string.IsNullOrWhiteSpace(XYCache.Instance.Cache.NowModelConfigFilePath);
        HasModel.SetActive(hasModel);
        NoModel.SetActive(!hasModel);
        if (hasModel)
        {
            if (loadConfig)
            {
                FileHelper.LoadNowModelConfig();
            }
            BindPool.DespawnAll();
            foreach (var data in XYPlugin.Instance.NowModelConfig.TriggerActionData)
            {
                var go = BindPool.Spawn(BindRT);
                go.GetComponent<UIActionBindItem>().SetData(data);
            }
            ActionPool.DespawnAll();
            foreach (var hotkey in XYCache.Instance.Cache.NowModelHotkeys)
            {
                var go = ActionPool.Spawn(ActionRT);
                go.GetComponent<UIHotkeyItem>().SetData(hotkey);
            }
        }
    }

    public void SaveConfig()
    {
        FileHelper.SaveNowModelConfig();
    }
}
