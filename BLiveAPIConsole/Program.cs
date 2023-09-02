using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLiveAPI;
using Newtonsoft.Json.Linq;

namespace BLiveAPIConsole
{
    internal class Program
    {
        public static ulong RoomID;
        public static ulong UID = 0;
        public static string SESS = "";
        public static BLiveApi api;

        private static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            if (!ulong.TryParse(args[0], out RoomID))
            {
                Console.WriteLine("输入的房间号必须为数字");
            }
            if (args.Length > 1)
            {
                if (!ulong.TryParse(args[1], out UID))
                {
                    Console.WriteLine("输入的UID必须为数字");
                }
            }
            if (args.Length > 2)
            {
                SESS = args[2];
            }

            api = new BLiveApi();
            api.OpAuthReply += OnOpAuthReply;
            api.OpHeartbeatReply += OpHeartbeatReplyEvent;
            api.WebSocketClose += WebSocketCloseEvent;
            api.WebSocketError += WebSocketErrorEvent;
            api.DecodeError += DecodeErrorEvent;
            api.DanmuMsg += OnDanmuMsg;
            api.SendGift += OnSendGiftEvent;
            api.SuperChatMessage += OnSuperChatMessage;
            api.UserToastMsg += OnUserToastMsg;
            api.OpSendSmsReply += OnWARNINGEvent;
            api.OpSendSmsReply += OnWATCHEDCHANGEEvent;
            api.OpSendSmsReply += OnGUARD_BUYEvent;
            try
            {
                api.Connect(RoomID, 2, UID, SESS);
            }
            catch
            {
            }
            Console.ReadLine();
        }

        private static void OnOpAuthReply(object sender, (JObject authReply, ulong? roomId, byte[] rawData) e)
        {
            // 连接到B站弹幕服务器
        }

        private static void OnSendGiftEvent(object sender, (JObject giftInfo, JObject blindInfo, string coinType, ulong userId, string userName, int guardLevel, string face, JObject rawData) e)
        {
            Console.WriteLine(SmsToStringHelper.SEND_GIFT(e.rawData));
        }

        private static void OnDanmuMsg(object sender, (string msg, ulong userId, string userName, int guardLevel, string face, JObject rawData) e)
        {
            Console.WriteLine(SmsToStringHelper.DANMU_MSG(e.rawData));
        }

        private static void OnUserToastMsg(object sender, (string roleName, int giftId, int guardLevel, int price, int num, string unit, ulong userId, string userName, JObject rawData) e)
        {
            Console.WriteLine(SmsToStringHelper.GUARD_BUY(e.rawData));
        }

        private static void OnSuperChatMessage(object sender, (string message, ulong id, int price, ulong userId, string userName, int guardLevel, string face, JObject rawData) e)
        {
            Console.WriteLine(SmsToStringHelper.SUPER_CHAT_MESSAGE(e.rawData));
        }

        [TargetCmd("WARNING")]
        private static void OnWARNINGEvent(object sender, (string cmd, string hitCmd, JObject rawData) e)
        {
            Console.WriteLine(SmsToStringHelper.WARNING(e.rawData));
        }

        [TargetCmd("WATCHED_CHANGE")]
        private static void OnWATCHEDCHANGEEvent(object sender, (string cmd, string hitCmd, JObject rawData) e)
        {
            Console.WriteLine(SmsToStringHelper.WATCHED_CHANGE(e.rawData));
        }

        [TargetCmd("GUARD_BUY")]
        private static void OnGUARD_BUYEvent(object sender, (string cmd, string hitCmd, JObject rawData) e)
        {
            // 此处消息的金额是原价，不准确
            //BuyJianDui buyJianDui = new BuyJianDui(e.rawData);
            //UniEvent.SendMessage(buyJianDui);
        }

        //用于接收心跳消息的方法
        private static void OpHeartbeatReplyEvent(object sender, (int heartbeatReply, byte[] rawData) e)
        {
        }

        //用于接收主动关闭消息的方法(使用者主动调用api.Close()时),同时会触发异常
        private static void WebSocketCloseEvent(object sender, (string message, int code) e)
        {
        }

        //用于接收被动关闭消息的方法(一般是网络错误等原因),同时会触发异常
        private static void WebSocketErrorEvent(object sender, (string message, int code) e)
        {
        }

        //用于接收API内部解码错误,一般情况下不会触发,除非B站改逻辑或其他特殊情况,此消息触发时不会引起异常
        //目前发现在不同的C#版本引入库时会出现不同的问题，所以暂时将此异常抛出并终止与直播间的连接
        //Unity使用本库时Brotli库不可用,在使用API的Connect方法时请将第二个参数设置为2
        //.NET项目使用本库时需要自己在NuGet安装 Newtonsoft.Json
        //.NET Framework项目目前使用无问题
        private static void DecodeErrorEvent(object sender, (string message, Exception e) e)
        {
        }
    }
}