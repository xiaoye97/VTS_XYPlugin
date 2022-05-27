// By 宵夜97
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPageInfo : MonoSingleton<UIPageInfo>
{
    public Text VersionText;
    private void Start()
    {
        VersionText.text = XYPlugin.Instance.Version;
    }

    public void OpenUrl(string url)
    {
        System.Diagnostics.Process.Start(url);
    }
}
