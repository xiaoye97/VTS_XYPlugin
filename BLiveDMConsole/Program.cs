using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BiliDMLib;

namespace BLiveDMConsole
{
    internal class Program
    {
        public static int RoomID;
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            if (args.Length != 1)
            {
                Console.WriteLine("必须有一个输入参数");
                return;
            }
            if (!int.TryParse(args[0], out RoomID))
            {
                Console.WriteLine("输入的参数必须为原始房间号数字");
            }
            ThreadStart childref = new ThreadStart(DanMuThread);
            Thread childThread = new Thread(childref);
            childThread.Start();
            Console.ReadLine();
        }

        public static async void DanMuThread()
        {
            DanmakuLoader loader = new DanmakuLoader();
            loader.ReceivedDanmaku += Loader_ReceivedDanmaku;
            loader.Disconnected += Loader_Disconnected;
            loader.ReceivedRoomCount += Loader_ReceivedRoomCount;
            await loader.ConnectAsync(RoomID);
        }

        private static void Loader_ReceivedRoomCount(object sender, ReceivedRoomCountArgs e)
        {
            // 人气值
            Console.WriteLine($"R$#**#${e.UserCount}");
        }

        private static void Loader_Disconnected(object sender, DisconnectEvtArgs e)
        {
            Environment.Exit(0);
        }

        private static void Loader_ReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            var dm = e.Danmaku;
            switch (dm.MsgType)
            {
                case MsgTypeEnum.Comment:
                    // 弹幕
                    Console.WriteLine($"D$#**#${dm.UserID}$#**#${dm.UserName}$#**#${dm.isAdmin}$#**#${dm.UserGuardLevel}$#**#${dm.MedalName}$#**#${dm.MedalLevel}$#**#${dm.CommentText}");
                    break;
                case MsgTypeEnum.GiftSend:
                    // 礼物
                    Console.WriteLine($"G$#**#${dm.UserID}$#**#${dm.UserName}$#**#${dm.GiftName}$#**#${dm.GiftCount}$#**#${dm.GiftCoinType}$#**#${dm.Price}$#**#${dm.FaceAddress}");
                    break;
                case MsgTypeEnum.GuardBuy:
                    // 舰队
                    Console.WriteLine($"J$#**#${dm.UserID}$#**#${dm.UserName}$#**#${dm.UserGuardLevel}$#**#${dm.GiftName}$#**#${dm.GiftCount}");
                    break;
                case MsgTypeEnum.SuperChat:
                    // SC
                    Console.WriteLine($"S$#**#${dm.UserID}$#**#${dm.UserName}$#**#${dm.Price}$#**#${dm.SCKeepTime}$#**#${dm.CommentText}");
                    break;
                case MsgTypeEnum.Warning:
                    // 超管警告
                    Console.WriteLine($"W$#**#${dm.CommentText}");
                    break;
                case MsgTypeEnum.WatchedChange:
                    // 看过人数
                    Console.WriteLine($"P$#**#${dm.WatchedCount}");
                    break;
            }
        }
    }
}
