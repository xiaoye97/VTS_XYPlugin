using SuperSimpleTcp;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using VTS_XYPlugin_Common;

namespace VTS_XYPlugin
{
    public class Bilibili : MonoSingleton<Bilibili>
    {
        public string ServerIPAddress = "127.0.0.1:9000";
        private SimpleTcpClient client;
        private BLiveDanmuParser danmuParser;

        private static float reConnectCD = 120;

        // 是否允许连接B站，当启动参数里含有nobili的时候，不连接B站
        public static bool CanConnectBili = true;

        public override void Init()
        {
            if (XYPlugin.CmdArgs.Contains("-nobili"))
            {
                CanConnectBili = false;
                XYLog.LogMessage($"当前已禁用连接Bilibili");
            }
            if (CanConnectBili)
            {
                danmuParser = new BLiveDanmuParser();
                danmuParser.OnDanmaku += DanmuParser_OnDanmaku;
                danmuParser.OnGift += DanmuParser_OnGift;
                danmuParser.OnGuardBuy += DanmuParser_OnGuardBuy;
                danmuParser.OnSuperChat += DanmuParser_OnSuperChat;
            }
        }

        public void Update()
        {
            if (CanConnectBili)
            {
                if (!client.IsConnected)
                {
                    reConnectCD -= Time.deltaTime;
                    if (reConnectCD < 0)
                    {
                        reConnectCD = 120;
                        XYLog.LogMessage($"开始尝试重连弹幕机");
                        Connect();
                    }
                }
            }
        }

        public void LoadOrCreateAddressConfig()
        {
            string doc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string path = $"{doc}/xiaoye97/XYDanMuShare/ServerIPAddress.txt";
            if (File.Exists(path))
            {
                ServerIPAddress = File.ReadAllText(path);
            }
            else
            {
                FileInfo f = new FileInfo(path);
                if (!f.Directory.Exists)
                {
                    f.Directory.Create();
                }
                ServerIPAddress = "127.0.0.1:9000";
                File.WriteAllText(path, ServerIPAddress);
            }
        }

        public void Connect()
        {
            LoadOrCreateAddressConfig();
            client = new SimpleTcpClient(ServerIPAddress);
            client.Events.Connected += Events_Connected;
            client.Events.Disconnected += Events_Disconnected;
            client.Events.DataReceived += Events_DataReceived;
            client.Connect();
        }

        private void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = Encoding.UTF8.GetString(e.Data.Array, 0, e.Data.Count);
            if (data.StartsWith("XYDanMuShareForCopyLiuDanMuJi;"))
            {
                var rawData = data.Replace("XYDanMuShareForCopyLiuDanMuJi;", "");
                danmuParser.ProcessNotice(rawData);
            }
        }

        private void Events_Disconnected(object sender, ConnectionEventArgs e)
        {
            XYLog.LogMessage($"与弹幕广播的连接已断开，即将尝试重连。");
            reConnectCD = 1f;
            client = null;
        }

        private void Events_Connected(object sender, ConnectionEventArgs e)
        {
            XYLog.LogMessage($"已连接到弹幕广播");
        }

        private void DanmuParser_OnGift(OpenBLive.Runtime.Data.SendGift sendGift)
        {
            var message = new BGiftMessage()
            {
                用户ID = sendGift.uid.ToString(),
                用户名 = sendGift.userName,
                礼物名 = sendGift.giftName,
                礼物数量 = (int)sendGift.giftNum,
                瓜子类型 = sendGift.paid ? BGiftCoinType.金瓜子 : BGiftCoinType.银瓜子,
                瓜子数量 = (int)sendGift.price,
                头像图片链接 = sendGift.userFace
            };
            BilibiliHeadCache.Instance.OnRecvGift(message);
            MessageCenter.Instance.Send(message);
        }

        private void DanmuParser_OnDanmaku(OpenBLive.Runtime.Data.Dm dm)
        {
            var message = new BDanMuMessage()
            {
                用户ID = dm.uid.ToString(),
                用户名 = dm.userName,
                舰队类型 = dm.guardLevel.ToString().ToJianDuiType(),
                粉丝牌名称 = dm.fansMedalName,
                粉丝牌等级 = (int)dm.fansMedalLevel,
                弹幕 = dm.msg
            };
            MessageCenter.Instance.Send(message);
        }

        private void DanmuParser_OnSuperChat(OpenBLive.Runtime.Data.SuperChat sc)
        {
            var message = new BSCMessage()
            {
                用户ID = sc.uid.ToString(),
                用户名 = sc.userName,
                金额 = (int)sc.rmb,
                持续时间 = (int)(sc.endTime - sc.startTime),
                SC = sc.message
            };
            MessageCenter.Instance.Send(message);
        }

        private void DanmuParser_OnGuardBuy(OpenBLive.Runtime.Data.Guard guard)
        {
            var message = new BBuyJianDuiMessage()
            {
                用户ID = guard.userInfo.uid.ToString(),
                用户名 = guard.userInfo.userName,
                开通类型 = guard.guardLevel.ToString().ToJianDuiType(),
                开通数量 = (int)guard.guardNum,
            };
            MessageCenter.Instance.Send(message);
        }
    }
}