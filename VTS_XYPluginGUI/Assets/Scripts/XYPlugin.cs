using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VTS_XYPlugin_Common;
using System.IO;

public class XYPlugin : MonoSingleton<XYPlugin>
{
    public string Version;
    /// <summary>
    /// 全局配置文件监听
    /// </summary>
    public XYFileWatcher GlobalConfigWatcher;
    /// <summary>
    /// 掉落物配置文件监听
    /// </summary>
    public XYFileWatcher DropItemDataBaseWatcher;
    /// <summary>
    /// 缓存文件监听
    /// </summary>
    public XYFileWatcher PluginCacheWatcher;
    /// <summary>
    /// 全局配置
    /// </summary>
    public XYGlobalConfig GlobalConfig;
    /// <summary>
    /// 掉落物配置
    /// </summary>
    public DropItemDataBase DropItemDataBase;

    public XYModelConfig NowModelConfig;
    public XYFileWatcher ModelConfigWatcher;

    public override void Awake()
    {
        base.Awake();
        Init();
    }

    void Update()
    {
        if (GlobalConfigWatcher != null)
        {
            GlobalConfigWatcher.Update(Time.deltaTime);
        }
        if (DropItemDataBaseWatcher != null)
        {
            DropItemDataBaseWatcher.Update(Time.deltaTime);
        }
    }

    public void Init()
    {
        // 监控全局配置文件
        GlobalConfigWatcher = new XYFileWatcher(XYPaths.GlobalConfigPath);
        GlobalConfigWatcher.OnFileModified += OnGlobalConfigFileModified;
        // 监控掉落物配置文件
        DropItemDataBaseWatcher = new XYFileWatcher(XYPaths.DropItemConfigPath);
        DropItemDataBaseWatcher.OnFileModified += OnDropItemConfigFileModified;
        OnGlobalConfigFileModified();
        OnDropItemConfigFileModified();
    }

    private void OnGlobalConfigFileModified()
    {
        FileHelper.LoadGlobalConfig();
        // 改变UI
        UIPageConfig.Instance.Refresh();
        UIPageGiftDrop.Instance.Refresh();
    }

    private void OnDropItemConfigFileModified()
    {
        FileHelper.LoadDropItemConfig();
        UIPageGiftDrop.Instance.Refresh();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
