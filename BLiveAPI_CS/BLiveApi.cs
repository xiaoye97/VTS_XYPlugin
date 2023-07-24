using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using BrotliSharpLib;
using Google.Protobuf;
using System.IO;

namespace BLiveAPI
{
    public class BLiveApi
    {
        public enum ClientOperation
        {
            OpHeartbeat = 2,
            OpAuth = 7
        }

        public enum ServerOperation
        {
            OpHeartbeatReply = 3,
            OpSendSmsReply = 5,
            OpAuthReply = 8
        }

        public readonly long? _uid;
        public readonly int? _roomId;
        public readonly ClientWebSocket _clientWebSocket;
        public const string WsHost = "wss://broadcastlv.chat.bilibili.com/sub";

        public static async Task Main(string[] args)
        {
            int roomid;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            if (args.Length != 1)
            {
                Console.WriteLine("必须有一个输入参数");
                return;
            }
            if (!int.TryParse(args[0], out roomid))
            {
                Console.WriteLine("输入的参数必须为原始房间号数字");
            }
            //System.IO.DirectoryInfo jsonDir = new System.IO.DirectoryInfo("json");
            //if (!jsonDir.Exists)
            //{
            //    jsonDir.Create();
            //}
            var api = new BLiveApi(roomid);
            await api.Connect();
        }

        public BLiveApi(int roomId)
        {
            (_roomId, _uid) = GetRoomIdAndUid(roomId);
            _clientWebSocket = new ClientWebSocket();
        }

        public static byte[] ToBigEndianBytes(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return bytes;
        }

        public static byte[] ToBigEndianBytes(short value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return bytes;
        }

        public static int BytesToInt(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return bytes.Length switch
            {
                2 => BitConverter.ToInt16(bytes, 0),
                4 => BitConverter.ToInt32(bytes, 0),
                _ => 0
            };
        }

        public static byte[] CreateWsPacket(ClientOperation operation, byte[] body)
        {
            var packetLength = 16 + body.Length;
            var result = new byte[packetLength];
            Buffer.BlockCopy(ToBigEndianBytes(packetLength), 0, result, 0, 4);
            Buffer.BlockCopy(ToBigEndianBytes((short)16), 0, result, 4, 2);
            Buffer.BlockCopy(ToBigEndianBytes((short)1), 0, result, 6, 2);
            Buffer.BlockCopy(ToBigEndianBytes((int)operation), 0, result, 8, 4);
            Buffer.BlockCopy(ToBigEndianBytes(1), 0, result, 12, 4);
            Buffer.BlockCopy(body, 0, result, 16, body.Length);
            return result;
        }

        public static byte[] GetChildFromProtoData(byte[] protoData, int target)
        {
            using (var input = new CodedInputStream(protoData))
            {
                while (!input.IsAtEnd)
                {
                    var tag = input.ReadTag();
                    var tagId = WireFormat.GetTagFieldNumber(tag);
                    if (tagId == target)
                    {
                        return input.ReadBytes().ToByteArray();
                    }
                    input.SkipLastField();
                }
            }
            return Array.Empty<byte>();
        }

        public void DecodeSms(JObject sms)
        {
            var cmd = (string)sms.GetValue("cmd");
            //SaveIfNotHas(cmd, sms);
            switch (cmd)
            {
                case "DANMU_MSG":
                    Console.WriteLine(SmsToStringHelper.DANMU_MSG(_uid.ToString(), sms));
                    break;

                case "LIVE_INTERACTIVE_GAME":
                    break;

                case "SEND_GIFT":
                    Console.WriteLine(SmsToStringHelper.SEND_GIFT(sms));
                    break;

                case "WATCHED_CHANGE":
                    Console.WriteLine(SmsToStringHelper.WATCHED_CHANGE(sms));
                    break;

                case "ONLINE_RANK_COUNT":
                    //Console.WriteLine($"高能榜人数：{sms["data"]["count"]}");
                    break;

                case "LIKE_INFO_V3_CLICK":
                    //Console.WriteLine($"{sms["data"]["uname"]} {sms["data"]["like_text"]}");
                    break;

                case "LIKE_INFO_V3_UPDATE":
                    //Console.WriteLine($"{sms["data"]["click_count"]} 人点赞");
                    break;

                case "INTERACT_WORD":
                    //Console.WriteLine($"{sms["data"]["uname"]} 进入直播间");
                    break;

                case "ENTRY_EFFECT":
                    //Console.WriteLine($"{sms["data"]["copy_writing"]}");
                    break;

                case "SUPER_CHAT_MESSAGE":
                case "SUPER_CHAT_MESSAGE_JPN":
                    Console.WriteLine(SmsToStringHelper.SUPER_CHAT_MESSAGE(sms));
                    break;

                case "GUARD_BUY":
                    // 舰长购买
                    Console.WriteLine(SmsToStringHelper.GUARD_BUY(sms));
                    break;
                case "WARNING":
                    Console.WriteLine(SmsToStringHelper.WARNING(sms));
                    break;

                case "LIVE_MULTI_VIEW_CHANGE":
                case "ONLINE_RANK_V2":
                case "ONLINE_RANK_TOP3":
                case "HOT_ROOM_NOTIFY":
                case "COMBO_END":
                case "COMBO_SEND":
                case "NOTICE_MSG":
                case "COMMON_NOTICE_DANMAKU":
                case "ROOM_REAL_TIME_MESSAGE_UPDATE": // 房间实时信息
                case "WIDGET_GIFT_STAR_PROCESS":
                case "WIDGET_BANNER":
                case "POPULAR_RANK_CHANGED":
                case "SPREAD_SHOW_FEET_V2":
                case "MESSAGEBOX_USER_GAIN_MEDAL":
                case "STOP_LIVE_ROOM_LIST":
                case "DANMU_AGGREGATION":
                case "USER_TOAST_MSG": // 用户的弹出信息
                case "POPULARITY_RED_POCKET_WINNER_LIST": // 红包获奖信息
                case "POPULARITY_RED_POCKET_START": // 红包开始信息
                case "RECOMMEND_CARD": // 推荐卡片
                case "GOTO_BUY_FLOW": // xxx正在去买
                case "AREA_RANK_CHANGED": // 分区航海排名改变
                    break;

                default:
                    //Console.WriteLine(sms);
                    break;
            }
        }

        public static void SaveIfNotHas(string cmd, JObject obj)
        {
            string path = $"json/{cmd}.json";
            if (!File.Exists(path))
            {
                File.WriteAllText(path, obj.ToString());
            }
        }

        public void DecodeMessage(ServerOperation operation, byte[] messageData)
        {
            switch (operation)
            {
                case ServerOperation.OpAuthReply:
                    //Console.WriteLine($"鉴权回复：{Encoding.UTF8.GetString(messageData)}");
                    break;

                case ServerOperation.OpHeartbeatReply:
                    //Console.WriteLine($"心跳回复：{BytesToInt(messageData)}");
                    break;

                case ServerOperation.OpSendSmsReply:
                    string json = Encoding.UTF8.GetString(messageData);
                    DecodeSms((JObject)JsonConvert.DeserializeObject(json));
                    break;

                default:
                    //Console.WriteLine("错误的ServerOperation！");
                    break;
            }
        }

        public void DecodePacket(byte[] packetData)
        {
            var header = new ArraySegment<byte>(packetData, 0, 16).ToArray();
            var body = new ArraySegment<byte>(packetData, 16, packetData.Length - 16).ToArray();
            var version = BytesToInt(new ArraySegment<byte>(header, 6, 2).ToArray());
            switch (version)
            {
                case 0:
                case 1:
                    var packetLength = BytesToInt(new ArraySegment<byte>(header, 0, 4).ToArray());
                    var operation = (ServerOperation)BytesToInt(new ArraySegment<byte>(header, 8, 4).ToArray());
                    DecodeMessage(operation, new ArraySegment<byte>(body, 0, packetLength - 16).ToArray());
                    if (packetData.Length > packetLength) DecodePacket(new ArraySegment<byte>(packetData, packetLength, packetData.Length - packetLength).ToArray());
                    break;

                case 3:
                    DecodePacket(Brotli.DecompressBuffer(body, 0, body.Length));
                    break;

                default:
                    //Console.WriteLine($"未知的Version{version}");
                    break;
            }
        }

        public async Task Connect()
        {
            await _clientWebSocket.ConnectAsync(new Uri(WsHost), CancellationToken.None);
            var authBody = new { uid = _uid, roomid = _roomId, protover = 3, platform = "web", type = 2 };
            var authPacket = CreateWsPacket(ClientOperation.OpAuth, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(authBody)));
            var heartPacket = CreateWsPacket(ClientOperation.OpHeartbeat, Array.Empty<byte>());
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(authPacket), WebSocketMessageType.Binary, true, CancellationToken.None);
            await Task.WhenAll(ReceiveMessage(), SendHeartbeat(heartPacket));
        }

        public async Task ReceiveMessage()
        {
            var buffer = new List<byte>();

            while (_clientWebSocket.State == WebSocketState.Open)
            {
                var tempBuffer = new byte[1024];
                var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(tempBuffer), CancellationToken.None);
                buffer.AddRange(new ArraySegment<byte>(tempBuffer, 0, result.Count));
                if (!result.EndOfMessage)
                {
                    continue;
                }
                DecodePacket(buffer.ToArray());
                buffer.Clear();
            }
        }

        public async Task SendHeartbeat(byte[] heartPacket)
        {
            while (_clientWebSocket.State == WebSocketState.Open)
            {
                await _clientWebSocket.SendAsync(new ArraySegment<byte>(heartPacket), WebSocketMessageType.Binary, true, CancellationToken.None);
                await Task.Delay(TimeSpan.FromSeconds(20));
            }
        }

        public static (int?, long?) GetRoomIdAndUid(int shortRoomId)
        {
            var url = $"https://api.live.bilibili.com/xlive/web-room/v1/index/getRoomBaseInfo?room_ids={shortRoomId}&req_biz=web/";
            var result = new HttpClient().GetStringAsync(url).Result;
            var jsonResult = (JObject)JsonConvert.DeserializeObject(result);
            var roomInfo = (JObject)jsonResult?["data"]?["by_room_ids"]?.Values().FirstOrDefault();
            var roomId = (int?)roomInfo?.GetValue("room_id");
            var uid = (long?)roomInfo?.GetValue("uid");
            return (roomId, uid);
        }
    }
}