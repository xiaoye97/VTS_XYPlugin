using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Google.Protobuf;
using Newtonsoft.Json.Linq;

namespace BLiveAPI;

/// <summary>
///     BLiveAPI的各种事件
/// </summary>
public abstract class BLiveEvents
{
    /// <inheritdoc />
    public delegate void BLiveEventHandler<in TEventArgs>(object sender, TEventArgs e);

    /// <inheritdoc cref="BLiveEvents" />
    protected BLiveEvents()
    {
        SendSmsReply += OnDanmuMsg;
        SendSmsReply += OnInteractWord;
        SendSmsReply += OnSendGift;
        SendSmsReply += OnSuperChatMessage;
        SendSmsReply += OnUserToastMsg;
    }

    /// <summary>
    ///     服务器回复的认证消息
    /// </summary>
    public event BLiveEventHandler<(JObject authReply, ulong? roomId, byte[] rawData)> OpAuthReply;

    /// <inheritdoc cref="OpAuthReply" />
    protected void OnOpAuthReply(JObject authReply, ulong? roomId, byte[] rawData)
    {
        OpAuthReply?.Invoke(this, (authReply, roomId, rawData));
    }

    /// <summary>
    ///     服务器回复的心跳消息
    /// </summary>
    public event BLiveEventHandler<(int heartbeatReply, byte[] rawData)> OpHeartbeatReply;

    /// <inheritdoc cref="OpHeartbeatReply" />
    protected void OnOpHeartbeatReply(int heartbeatReply, byte[] rawData)
    {
        OpHeartbeatReply?.Invoke(this, (heartbeatReply, rawData));
    }

    /// <summary>
    ///     服务器发送的SMS消息
    /// </summary>
    public event BLiveEventHandler<(string cmd, string hitCmd, JObject rawData)> OpSendSmsReply;

    private void InvokeOpSendSmsReply(JObject rawData, bool hit)
    {
        if (OpSendSmsReply is null) return;
        var waitInvokeList = OpSendSmsReply.GetInvocationList().ToList();
        var cmd = (string)rawData["cmd"];
        foreach (var invocation in OpSendSmsReply.GetInvocationList())
        {
            var targetCmdAttribute = invocation.Method.GetCustomAttributes<TargetCmdAttribute>().FirstOrDefault();
            if (targetCmdAttribute is null)
            {
                invocation.DynamicInvoke(this, (cmd, "ALL", rawData));
                waitInvokeList.Remove(invocation);
            }
            else if (targetCmdAttribute.HasCmd(cmd))
            {
                invocation.DynamicInvoke(this, (cmd, cmd, rawData));
                waitInvokeList.Remove(invocation);
                hit = true;
            }
            else if (targetCmdAttribute.HasCmd("ALL"))
            {
                invocation.DynamicInvoke(this, (cmd, "ALL", rawData));
                waitInvokeList.Remove(invocation);
            }
            else if (!targetCmdAttribute.HasCmd("OTHERS"))
            {
                waitInvokeList.Remove(invocation);
            }
        }

        if (hit) return;
        foreach (var invocation in waitInvokeList) invocation.DynamicInvoke(this, (cmd, "OTHERS", rawData));
    }

    private event BLiveSmsEventHandler SendSmsReply;

    private bool InvokeSendSmsReply(JObject rawData)
    {
        if (SendSmsReply is null) return false;
        var cmd = (string)rawData["cmd"];
        return (from invocation in SendSmsReply.GetInvocationList()
            let targetCmdAttribute = invocation.Method.GetCustomAttributes<TargetCmdAttribute>().FirstOrDefault()
            where targetCmdAttribute != null && targetCmdAttribute.HasCmd(cmd)
            select invocation).Aggregate(false, (current, invocation) => (bool)invocation.DynamicInvoke(rawData) || current);
    }

    /// <inheritdoc cref="OpSendSmsReply" />
    protected void OnOpSendSmsReply(JObject rawData)
    {
        InvokeOpSendSmsReply(rawData, InvokeSendSmsReply(rawData));
    }

    /// <summary>
    ///     弹幕消息,guardLevel 0:普通观众 1:总督 2:提督 3:舰长
    /// </summary>
    public event BLiveEventHandler<(string msg, ulong userId, string userName, int guardLevel, string face, JObject rawData)> DanmuMsg;

    private static byte[] GetChildFromProtoData(byte[] protoData, int target)
    {
        using (var input = new CodedInputStream(protoData))
        {
            while (!input.IsAtEnd)
            {
                var tag = input.ReadTag();
                var tagId = WireFormat.GetTagFieldNumber(tag);
                if (tagId == target) return input.ReadBytes().ToByteArray();
                input.SkipLastField();
            }
        }

        return Array.Empty<byte>();
    }

    [TargetCmd("DANMU_MSG")]
    private bool OnDanmuMsg(JObject rawData)
    {
        var msg = (string)rawData["info"][1];
        var userId = (ulong)rawData["info"][2]?[0];
        var userName = (string)rawData["info"][2]?[1];
        var guardLevel = (int)rawData["info"][7];
        var protoData = Convert.FromBase64String(rawData["dm_v2"].ToString());
        var face = Encoding.UTF8.GetString(GetChildFromProtoData(GetChildFromProtoData(protoData, 20), 4));
        DanmuMsg?.Invoke(this, (msg, userId, userName, guardLevel, face, rawData));
        return DanmuMsg is not null;
    }

    /// <summary>
    ///     观众进房消息,privilegeType 0:普通观众 1:总督 2:提督 3:舰长
    /// </summary>
    public event BLiveEventHandler<(int privilegeType, ulong userId, string userName, JObject rawData)> InteractWord;

    [TargetCmd("INTERACT_WORD")]
    private bool OnInteractWord(JObject rawData)
    {
        var privilegeType = (int)rawData["data"]["privilege_type"];
        var userId = (ulong)rawData["data"]["uid"];
        var userName = (string)rawData["data"]["uname"];
        InteractWord?.Invoke(this, (privilegeType, userId, userName, rawData));
        return InteractWord is not null;
    }

    /// <summary>
    ///     投喂礼物事件 giftInfo:礼物信息 blindInfo:盲盒礼物信息,如果此礼物不是盲盒爆出则为null coinType:区别是金瓜子礼物还是银瓜子礼物 guardLevel 0:普通观众 1:总督 2:提督 3:舰长
    /// </summary>
    public event BLiveEventHandler<( JObject giftInfo, JObject blindInfo, string coinType, ulong userId, string userName, int guardLevel, string face, JObject rawData)> SendGift;

    [TargetCmd("SEND_GIFT")]
    private bool OnSendGift(JObject rawData)
    {
        var data = rawData["data"];
        var blind = data["blind_gift"];
        var userId = (ulong)data["uid"];
        var userName = (string)data["uname"];
        var guardLevel = (int)data["guard_level"];
        var face = (string)data["face"];
        var coinType = (string)data["coin_type"];
        var giftInfo = JObject.FromObject(new { action = data["action"], giftId = data["giftId"], giftName = data["giftName"], price = data["price"] });
        var blindInfo = blind?.Type is JTokenType.Null
            ? null
            : JObject.FromObject(new
            {
                action = blind?.SelectToken("gift_action"),
                giftId = blind?.SelectToken("original_gift_id"),
                giftName = blind?.SelectToken("original_gift_name"),
                price = blind?.SelectToken("original_gift_price")
            });
        SendGift?.Invoke(this, (giftInfo, blindInfo, coinType, userId, userName, guardLevel, face, rawData));
        return SendGift is not null;
    }

    /// <summary>
    ///     SC消息事件 guardLevel 0:普通观众 1:总督 2:提督 3:舰长
    /// </summary>
    public event BLiveEventHandler<(string message, ulong id, int price, ulong userId, string userName, int guardLevel, string face, JObject rawData)> SuperChatMessage;

    [TargetCmd("SUPER_CHAT_MESSAGE")]
    private bool OnSuperChatMessage(JObject rawData)
    {
        var message = (string)rawData["data"]["message"];
        var price = (int)rawData["data"]["price"];
        var id = (ulong)rawData["data"]["id"];
        var userId = (ulong)rawData["data"]["uid"];
        var face = (string)rawData["data"]["user_info"]?["face"];
        var userName = (string)rawData["data"]["user_info"]?["uname"];
        var guardLevel = (int)rawData["data"]["user_info"]?["guard_level"];
        SuperChatMessage?.Invoke(this, (message, id, price, userId, userName, guardLevel, face, rawData));
        return SuperChatMessage is not null;
    }

    /// <summary>
    ///     上舰消息事件 price的单位是金瓜子
    /// </summary>
    public event BLiveEventHandler<(string roleName, int giftId, int guardLevel, int price, int num, string unit, ulong userId, string userName, JObject rawData)> UserToastMsg;

    [TargetCmd("USER_TOAST_MSG")]
    private bool OnUserToastMsg(JObject rawData)
    {
        var roleName = (string)rawData["data"]["role_name"];
        var giftId = (int)rawData["data"]["gift_id"];
        var guardLevel = (int)rawData["data"]["guard_level"];
        var price = (int)rawData["data"]["price"];
        var num = (int)rawData["data"]["num"];
        var unit = (string)rawData["data"]["unit"];
        var userId = (ulong)rawData["data"]["uid"];
        var userName = (string)rawData["data"]["username"];
        UserToastMsg?.Invoke(this, (roleName, giftId, guardLevel, price, num, unit, userId, userName, rawData));
        return UserToastMsg is not null;
    }

    /// <summary>
    ///     WebSocket异常关闭
    /// </summary>
    public event BLiveEventHandler<(string message, int code)> WebSocketError;

    /// <inheritdoc cref="WebSocketError" />
    protected void OnWebSocketError(string message, int code)
    {
        WebSocketError?.Invoke(this, (message, code));
    }

    /// <summary>
    ///     WebSocket主动关闭
    /// </summary>
    public event BLiveEventHandler<(string message, int code)> WebSocketClose;

    /// <inheritdoc cref="WebSocketClose" />
    protected void OnWebSocketClose(string message, int code)
    {
        WebSocketClose?.Invoke(this, (message, code));
    }

    /// <summary>
    ///     解析消息过程出现的错误，不影响WebSocket正常运行，所以不抛出异常(当前版本暂时会抛出)
    /// </summary>
    public event BLiveEventHandler<(string message, Exception e)> DecodeError;

    /// <inheritdoc cref="DecodeError" />
    protected void OnDecodeError(string message, Exception e)
    {
        DecodeError?.Invoke(this, (message, e));
    }

    private delegate bool BLiveSmsEventHandler(JObject rawData);
}