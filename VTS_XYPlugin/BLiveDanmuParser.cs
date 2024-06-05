using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OpenBLive.Runtime.Data;
using System;

namespace VTS_XYPlugin
{
    public delegate void ReceiveDanmakuEvent(Dm dm);

    public delegate void ReceiveGiftEvent(SendGift sendGift);

    public delegate void ReceiveGuardBuyEvent(Guard guard);

    public delegate void ReceiveSuperChatEvent(SuperChat e);

    public delegate void ReceiveSuperChatDelEvent(SuperChatDel e);

    public delegate void ReceiveRawNotice(string raw, JObject jObject);

    public class BLiveDanmuParser
    {
        public event ReceiveDanmakuEvent OnDanmaku;

        public event ReceiveGiftEvent OnGift;

        public event ReceiveGuardBuyEvent OnGuardBuy;

        public event ReceiveSuperChatEvent OnSuperChat;

        public event ReceiveSuperChatDelEvent OnSuperChatDel;

        public event ReceiveRawNotice ReceiveNotice;

        public BLiveDanmuParser()
        {
        }

        /// <summary>
        /// 从开放平台SDK直接拿的代码
        /// </summary>
        /// <param name="rawMessage"></param>
        public void ProcessNotice(string rawMessage)
        {
            var json = JObject.Parse(rawMessage);
            ReceiveNotice?.Invoke(rawMessage, json);
            var data = json["data"].ToString();
            try
            {
                switch (json["cmd"]?.ToString())
                {
                    case "LIVE_OPEN_PLATFORM_DM":
                        var dm = JsonConvert.DeserializeObject<Dm>(data);
                        OnDanmaku?.Invoke(dm);
                        break;

                    case "LIVE_OPEN_PLATFORM_SUPER_CHAT":
                        var superChat = JsonConvert.DeserializeObject<SuperChat>(data);
                        OnSuperChat?.Invoke(superChat);
                        break;

                    case "LIVE_OPEN_PLATFORM_SUPER_CHAT_DEL":
                        var superChatDel = JsonConvert.DeserializeObject<SuperChatDel>(data);
                        OnSuperChatDel?.Invoke(superChatDel);
                        break;

                    case "LIVE_OPEN_PLATFORM_SEND_GIFT":
                        var gift = JsonConvert.DeserializeObject<SendGift>(data);
                        OnGift?.Invoke(gift);
                        break;

                    case "LIVE_OPEN_PLATFORM_GUARD":
                        var guard = JsonConvert.DeserializeObject<Guard>(data);
                        OnGuardBuy?.Invoke(guard);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("json数据解析异常 rawMessage: " + rawMessage + e.Message);
            }
        }
    }
}