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

public class UIPageSctipt : MonoSingleton<UIPageSctipt>
{
    public LeanGameObjectPool ScriptPool;
    public RectTransform ScriptRT;

    public void Refresh()
    {
        ScriptPool.DespawnAll();
        foreach (var item in XYCache.Instance.Cache.InstallExScripts)
        {
            if (item.Name == "VTS_XYPlugin")
            {
                continue;
            }
            var go = ScriptPool.Spawn(ScriptRT);
            var ui = go.GetComponent<Text>();
            string text = "";
            text += $"<color=#7FD6FD>名称:</color>{item.Name}\n";
            text += $"<color=#7FD6FD>版本:</color>{item.Version}\n";
            text += $"<color=#7FD6FD>作者:</color>{item.Author}\n";
            text += $"<color=#7FD6FD>描述:</color>{item.Description}";
            ui.text = text;
        }
    }
}
