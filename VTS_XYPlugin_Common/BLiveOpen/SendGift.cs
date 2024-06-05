using System;
using Newtonsoft.Json;

namespace OpenBLive.Runtime.Data
{
    /// <summary>
    /// 礼物数据 https://open-live.bilibili.com/doc/2/2/1
    /// </summary>
    [Serializable]
    public struct SendGift
    {
        /// <summary>
        /// 房间号
        /// </summary>
        [JsonProperty("room_id")] public long roomId;

        /// <summary>
        /// 送礼用户UID
        /// </summary>
        [JsonProperty("uid")] public long uid;

        /// <summary>
        /// 送礼用户昵称
        /// </summary>
        [JsonProperty("uname")] public string userName;

        /// <summary>
        /// 送礼用户头像
        /// </summary>
        [JsonProperty("uface")] public string userFace;

        /// <summary>
        /// 道具id(盲盒:爆出道具id)
        /// </summary>
        [JsonProperty("gift_id")] public long giftId;

        /// <summary>
        /// 道具名(盲盒:爆出道具名)
        /// </summary>
        [JsonProperty("gift_name")] public string giftName;

        /// <summary>
        /// 赠送道具数量
        /// </summary>
        [JsonProperty("gift_num")] public long giftNum;

        /// <summary>
        /// 支付金额(1000 = 1元 = 10电池),盲盒:爆出道具的价值
        /// </summary>
        [JsonProperty("price")] public long price;

        /// <summary>
        /// 是否真的花钱(电池道具)
        /// </summary>
        [JsonProperty("paid")] public bool paid;

        /// <summary>
        /// 粉丝勋章等级
        /// </summary>
        [JsonProperty("fans_medal_level")] public long fansMedalLevel;

        /// <summary>
        /// 粉丝勋章名
        /// </summary>
        [JsonProperty("fans_medal_name")] public string fansMedalName;

        /// <summary>
        /// 佩戴的粉丝勋章佩戴状态
        /// </summary>
        [JsonProperty("fans_medal_wearing_status")]
        public bool fansMedalWearingStatus;

        /// <summary>
        /// 大航海等级
        /// </summary>
        [JsonProperty("guard_level")] public long guardLevel;

        /// <summary>
        /// 收礼时间秒级时间戳
        /// </summary>
        [JsonProperty("timestamp")] public long timestamp;

        /// <summary>
        /// 主播信息
        /// </summary>
        [JsonProperty("anchor_info")] public AnchorInfo anchorInfo;
    }
}