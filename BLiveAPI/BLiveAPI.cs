using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrotliSharpLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BLiveAPI;

/// <summary>
///     B站直播间弹幕接口
/// </summary>
public class BLiveApi : BLiveEvents
{
    private const string WsHost = "wss://broadcastlv.chat.bilibili.com/sub";
    private ClientWebSocket _clientWebSocket;
    private ulong? _roomId;
    private CancellationTokenSource _webSocketCancelToken;


    private static int BytesToInt(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes.Length switch
        {
            2 => BitConverter.ToInt16(bytes, 0),
            4 => BitConverter.ToInt32(bytes, 0),
            _ => throw new InvalidBytesLengthException()
        };
    }

    private void DecodeMessage(ServerOperation operation, byte[] messageData)
    {
        switch (operation)
        {
            case ServerOperation.OpAuthReply:
                OnOpAuthReply((JObject)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(messageData)), _roomId, messageData);
                break;
            case ServerOperation.OpHeartbeatReply:
                OnOpHeartbeatReply(BytesToInt(messageData), messageData);
                break;
            case ServerOperation.OpSendSmsReply:
                OnOpSendSmsReply((JObject)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(messageData)));
                break;
            default:
                throw new UnknownServerOperationException(operation);
        }
    }

    private void DecodePacket(byte[] packetData)
    {
        while (true)
        {
            var header = new ArraySegment<byte>(packetData, 0, 16).ToArray();
            var body = new ArraySegment<byte>(packetData, 16, packetData.Length - 16).ToArray();
            var version = BytesToInt(new ArraySegment<byte>(header, 6, 2).ToArray());
            switch (version)
            {
                case 0:
                case 1:
                    var firstPacketLength = BytesToInt(new ArraySegment<byte>(header, 0, 4).ToArray());
                    var operation = (ServerOperation)BytesToInt(new ArraySegment<byte>(header, 8, 4).ToArray());
                    DecodeMessage(operation, new ArraySegment<byte>(body, 0, firstPacketLength - 16).ToArray());
                    if (packetData.Length > firstPacketLength)
                    {
                        packetData = new ArraySegment<byte>(packetData, firstPacketLength, packetData.Length - firstPacketLength).ToArray();
                        continue;
                    }

                    break;
                case 2:
                    using (var resultStream = new MemoryStream())
                    using (var packetStream = new MemoryStream(body, 2, body.Length - 2))
                    using (var deflateStream = new DeflateStream(packetStream, CompressionMode.Decompress))
                    {
                        deflateStream.CopyTo(resultStream);
                        packetData = resultStream.ToArray();
                        continue;
                    }

                case 3:
                    packetData = Brotli.DecompressBuffer(body, 0, body.Length);
                    continue;
                default:
                    throw new UnknownVersionException(version);
            }

            break;
        }
    }

    private async Task ReceiveMessage()
    {
        var buffer = new List<byte>();
        while (_clientWebSocket.State == WebSocketState.Open)
        {
            var tempBuffer = new byte[1024];
            var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(tempBuffer), _webSocketCancelToken.Token);
            buffer.AddRange(new ArraySegment<byte>(tempBuffer, 0, result.Count));
            if (!result.EndOfMessage) continue;
            try
            {
                DecodePacket(buffer.ToArray());
            }
            catch (Exception e)
            {
                OnDecodeError(e.Message, e);
                _webSocketCancelToken?.Cancel();
                throw;
            }
            finally
            {
                buffer.Clear();
            }
        }

        throw new OperationCanceledException();
    }

    private async Task SendHeartbeat(ArraySegment<byte> heartPacket)
    {
        while (_clientWebSocket.State == WebSocketState.Open)
        {
            await _clientWebSocket.SendAsync(heartPacket, WebSocketMessageType.Binary, true, _webSocketCancelToken.Token);
            await Task.Delay(TimeSpan.FromSeconds(20), _webSocketCancelToken.Token);
        }

        throw new OperationCanceledException();
    }

    private static byte[] ToBigEndianBytes(int value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }

    private static byte[] ToBigEndianBytes(short value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }

    private static ArraySegment<byte> CreateWsPacket(ClientOperation operation, byte[] body)
    {
        var packetLength = 16 + body.Length;
        var result = new byte[packetLength];
        Buffer.BlockCopy(ToBigEndianBytes(packetLength), 0, result, 0, 4);
        Buffer.BlockCopy(ToBigEndianBytes((short)16), 0, result, 4, 2);
        Buffer.BlockCopy(ToBigEndianBytes((short)1), 0, result, 6, 2);
        Buffer.BlockCopy(ToBigEndianBytes((int)operation), 0, result, 8, 4);
        Buffer.BlockCopy(ToBigEndianBytes(1), 0, result, 12, 4);
        Buffer.BlockCopy(body, 0, result, 16, body.Length);
        return new ArraySegment<byte>(result);
    }

    private static ulong? GetRoomId(ulong shortRoomId)
    {
        try
        {
            var url = $"https://api.live.bilibili.com/xlive/web-room/v1/index/getRoomBaseInfo?room_ids={shortRoomId}&req_biz=web/";
            var result = new HttpClient().GetStringAsync(url).Result;
            var jsonResult = (JObject)JsonConvert.DeserializeObject(result);
            var roomInfo = (JObject)jsonResult?["data"]?["by_room_ids"]?.Values().FirstOrDefault();
            var roomId = (ulong?)roomInfo?.GetValue("room_id");
            if (roomId is null) throw new InvalidRoomIdException();
            return roomId;
        }
        catch (InvalidRoomIdException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw new DomainNameEncodingException();
        }
        catch
        {
            throw new NetworkException();
        }
    }

    private static string GetBuVid()
    {
        try
        {
            var result = new HttpClient().GetAsync("https://data.bilibili.com/v/").Result;
            return result.Headers.GetValues("Set-Cookie").First().Split(';').First().Split('=').Last();
        }
        catch (ArgumentException)
        {
            throw new DomainNameEncodingException();
        }
        catch
        {
            throw new NetworkException();
        }
    }

    private static string GetKey(ulong? roomId, string sessdata)
    {
        try
        {
            var client = new HttpClient(new HttpClientHandler { UseCookies = false });
            if (sessdata is not null) client.DefaultRequestHeaders.Add("Cookie", $"SESSDATA={sessdata}");
            var result = client.GetStringAsync($"https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo?id={roomId}&type=0").Result;
            var jsonResult = (JObject)JsonConvert.DeserializeObject(result);
            return (string)jsonResult?["data"]?["token"];
        }
        catch (ArgumentException)
        {
            throw new DomainNameEncodingException();
        }
        catch
        {
            throw new NetworkException();
        }
    }

    /// <summary>
    ///     关闭当前对象中的WebSocket
    /// </summary>
    public async Task Close()
    {
        _webSocketCancelToken?.Cancel();
        if (_clientWebSocket is not null && _clientWebSocket.State == WebSocketState.Open)
            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None);
    }

    /// <summary>
    ///     连接指定的直播间
    /// </summary>
    /// <param name="roomId">直播间id,可以是短位id</param>
    /// <param name="protoVer">压缩类型2:zlib,3:brotli<br />unity中请使用zlib,使用brotli会导致unity闪退假死等问题!!!!</param>
    /// <param name="uid">使用者的B站uid</param>
    /// <param name="sessdata">使用者的B站Cookie中的SESSDATA</param>
    public async Task Connect(ulong roomId, int protoVer, ulong uid = 0, string sessdata = null)
    {
        if (_webSocketCancelToken is not null) throw new ConnectAlreadyRunningException();
        if (protoVer is not (2 or 3)) throw new InvalidProtoVerException();
        try
        {
            _webSocketCancelToken = new CancellationTokenSource();
            _roomId = GetRoomId(roomId);
            _clientWebSocket = new ClientWebSocket();
            var authBody = new
            {
                uid = sessdata is null ? 0 : uid, roomid = _roomId, protover = protoVer, buvid = GetBuVid(), platform = "web", type = 2, key = GetKey(_roomId, uid == 0 ? null : sessdata)
            };
            var authPacket = CreateWsPacket(ClientOperation.OpAuth, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(authBody)));
            var heartPacket = CreateWsPacket(ClientOperation.OpHeartbeat, Array.Empty<byte>());
            await _clientWebSocket.ConnectAsync(new Uri(WsHost), _webSocketCancelToken.Token);
            await _clientWebSocket.SendAsync(authPacket, WebSocketMessageType.Binary, true, _webSocketCancelToken.Token);
            await Task.WhenAll(ReceiveMessage(), SendHeartbeat(heartPacket));
        }
        catch (OperationCanceledException)
        {
            OnWebSocketClose("WebSocket主动关闭", 0);
            throw new WebSocketCloseException();
        }
        catch (WebSocketException)
        {
            OnWebSocketError("WebSocket异常关闭", -1);
            _webSocketCancelToken?.Cancel();
            throw new WebSocketErrorException();
        }
        finally
        {
            _roomId = null;
            _clientWebSocket = null;
            _webSocketCancelToken = null;
        }
    }

    private enum ClientOperation
    {
        OpHeartbeat = 2,
        OpAuth = 7
    }

    private enum ServerOperation
    {
        OpHeartbeatReply = 3,
        OpSendSmsReply = 5,
        OpAuthReply = 8
    }
}