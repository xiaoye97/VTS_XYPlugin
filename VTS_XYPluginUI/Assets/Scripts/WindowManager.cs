using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kirurobo;
using System.Runtime.InteropServices;
using System;

public class WindowManager : MonoSingleton<WindowManager>
{
    #region 方法导入

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    // 还原
    private const int SW_SHOWRESTORE = 1;

    // {最小化, 激活}
    private const int SW_SHOWMINIMIZED = 2;

    // 最大化
    private const int SW_SHOWMAXIMIZED = 3;

    #endregion 方法导入

    private UniWindowController UWC;

    public override void Awake()
    {
        base.Awake();
        // 如果不在编辑器，就加载透明窗口控制器
        if (!Application.isEditor)
        {
            UWC = gameObject.AddComponent<UniWindowController>();
            UWC.isTransparent = true;
            UWC.isHitTestEnabled = false;
            SetResolution(Screen.currentResolution.width * 925 / 1000, Screen.currentResolution.height * 925 / 1000);
        }
    }

    public void SetResolution(int width, int height)
    {
        if (UWC != null)
        {
            UWC.windowSize = new Vector2(width, height);
        }
        else
        {
            Screen.SetResolution(width, height, false);
        }
    }

    /// <summary>
    /// 最小化窗口
    /// </summary>
    public void MinimizeWindow()
    {
        if (!Application.isEditor)
        {
            ShowWindow(GetForegroundWindow(), SW_SHOWMINIMIZED);
        }
    }

    /// <summary>
    /// 最大化窗口
    /// </summary>
    public void MaximizeWindow()
    {
        if (!Application.isEditor)
        {
            ShowWindow(GetForegroundWindow(), SW_SHOWMAXIMIZED);
        }
    }

    /// <summary>
    /// 还原窗口
    /// </summary>
    public void RestoreWindow()
    {
        if (!Application.isEditor)
        {
            ShowWindow(GetForegroundWindow(), SW_SHOWRESTORE);
        }
    }

    /// <summary>
    /// 关闭窗口
    /// </summary>
    public void CloseWindow()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}