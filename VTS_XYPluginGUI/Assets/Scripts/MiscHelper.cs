// By 宵夜97
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public static class MiscHelper
{
    /// <summary>
    /// 根本文本设置值，如果没有找到对应的文本，则不改变值
    /// </summary>
    /// <param name="dropdown"></param>
    /// <param name="value"></param>
    public static void SetValue(this Dropdown dropdown, string value)
    {
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            if (dropdown.options[i].text == value)
            {
                dropdown.value = i;
                break;
            }
        }
    }

    /// <summary>
    /// 获取当前选项的文本
    /// </summary>
    /// <param name="dropdown"></param>
    /// <returns></returns>
    public static string GetText(this Dropdown dropdown)
    {
        return dropdown.options[dropdown.value].text;
    }

    /// <summary>
    /// 获取枚举
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dropdown"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static T GetEnum<T>(this Dropdown dropdown) where T : Enum
    {
        string text = dropdown.GetText();
        try
        {
            var value = Enum.Parse(typeof(T), text);
            return (T)value;
        }
        catch
        {
        }
        return default(T);
    }
}
