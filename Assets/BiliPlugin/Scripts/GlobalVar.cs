using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalVar
{
    public static JSONObject Data = new JSONObject();

    public static void SendData(string key)
    {
        if (Data.HasField(key))
        {
            BiliPlugin.Instance.SendGlobalVar(key, Data[key].ToString(), (v) => { }, (e) => { });
        }
    }
}
