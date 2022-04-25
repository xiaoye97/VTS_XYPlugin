using System;
using UnityEngine;
using WebSocketSharp;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace VTS_XYPluginGameSide
{
    public class XYAPI
    {
        /// <summary>
        /// 保存插件使用的API名字
        /// </summary>
        public static List<string> PluginAPINameList = new List<string>()
        {
            "XYDropItemRequest",
            "XYRefreshDropItemRequest",
            "XYSetModelColliderRequest"
        };

        public static ConcurrentQueue<APIBaseMessage<APIMessageEmpty>> inboundMessageQueue = new ConcurrentQueue<APIBaseMessage<APIMessageEmpty>>();
        public static void Update()
        {
            while (!inboundMessageQueue.IsEmpty)
            {
                APIBaseMessage<APIMessageEmpty> apibaseMessage;
                if (inboundMessageQueue.TryDequeue(out apibaseMessage))
                {
                    string wholePayloadAsString = apibaseMessage.data.wholePayloadAsString;
                    string messageType = apibaseMessage.messageType;
                    if (messageType != null)
                    {
                        switch (messageType)
                        {
                            case "XYDropItemRequest":
                                XYDropItem(wholePayloadAsString);
                                break;
                            case "XYRefreshDropItemRequest":
                                XYRefreshDropItem();
                                break;
                            case "XYSetModelColliderRequest":
                                XYSetModelCollider(wholePayloadAsString);
                                break;
                        }
                    }
                }
            }
        }

        private static void XYDropItem(string data)
        {
            // 这里有点奇怪，使用JsonUtility直接解析会丢失泛型的data，所以手动解析一下
            JSONObject json = new JSONObject(data);
            XYDropItemRequest apiData = JsonUtility.FromJson<XYDropItemRequest>(json["data"].ToString());
            XYPlugin.Instance.Log($"接收到物品掉落信息:{apiData.ImageName}x{apiData.DropCount}");
            XYPlugin.Instance.DropItemManager.DropItem(apiData.ImageName, apiData.DropCount);
        }

        private static void XYRefreshDropItem()
        {
            XYPlugin.Instance.Log($"接收到刷新物品掉落信息请求");
            XYPlugin.Instance.DropItemManager.ReloadDropItemData();
        }
        
        private static void XYSetModelCollider(string data)
        {
            JSONObject json = new JSONObject(data);
            XYSetModelColliderRequest apiData = JsonUtility.FromJson<XYSetModelColliderRequest>(json["data"].ToString());
            XYPlugin.Instance.Log($"接收到设置碰撞体请求");
            XYPlugin.Instance.DropItemManager.SetModelCollider(apiData);
        }
    }
}
