using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTS_XYPluginGameSide
{
    public static class GlobalVar
    {
        public static JSONObject Data = new JSONObject();
        public static Dictionary<string, Action> RecvDataActions = new Dictionary<string, Action>();

        public static void OnRecvData(string key, string data)
        {
            JSONObject obj = new JSONObject(data);
            Data.SetField(key, obj);
            XYPlugin.Instance.Log($"GlobalVar:将{key}设置为{data}");
            foreach (var kv in RecvDataActions)
            {
                if (kv.Value != null)
                {
                    kv.Value();
                }
            }
        }
    }
}
