using System;
using Newtonsoft.Json;

namespace OpenBLive.Runtime.Data
{
    /// <summary>
    /// 付费留言数据 https://open-live.bilibili.com/doc/2/2/3
    /// </summary>
    [Serializable]
    public struct SuperChat
    {
        /// <summary>
        /// 直播间ID
        /// </summary>
        [JsonProperty("room_id")] public long roomId;

        /// <summary>
        /// 购买用户UID
        /// </summary>
        [JsonProperty("uid")] public long uid;

        /// <summary>
        /// 购买的用户昵称
        /// </summary>
        [JsonProperty("uname")] public string userName;

        /// <summary>
        /// 购买用户头像
        /// </summary>
        [JsonProperty("uface")] public string userFace;

        /// <summary>
        /// 留言id(风控场景下撤回留言需要)
        /// </summary>
        [JsonProperty("message_id")] public long messageId;

        /// <summary>
        /// 留言内容
        /// </summary>
        [JsonProperty("message")] public string message;

        /// <summary>
        /// 支付金额(元)
        /// </summary>
        [JsonProperty("rmb")] public long rmb;

        /// <summary>
        /// 赠送时间秒级
        /// </summary>
        [JsonProperty("timestamp")] public long timeStamp;

        /// <summary>
        /// 生效开始时间
        /// </summary>
        [JsonProperty("start_time")] public long startTime;

        /// <summary>
        /// 生效结束时间
        /// </summary>
        [JsonProperty("end_time")] public long endTime;

        /// <summary>
        /// 对应房间大航海等级
        /// </summary>
        [JsonProperty("guard_level")] public long guardLevel;

        /// <summary>
        /// 对应房间勋章信息
        /// </summary>
        [JsonProperty("fans_medal_level")] public long fansMedalLevel;

        /// <summary>
        /// 对应房间勋章名字
        /// </summary>
        [JsonProperty("fans_medal_name")] public string fansMedalName;

        /// <summary>
        /// 当前佩戴的粉丝勋章佩戴状态
        /// </summary>
        [JsonProperty("fans_medal_wearing_status")]
        public bool fansMedalWearingStatus;
    }
}