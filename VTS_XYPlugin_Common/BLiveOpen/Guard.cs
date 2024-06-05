using System;
using Newtonsoft.Json;

namespace OpenBLive.Runtime.Data
{
    /// <summary>
    /// 大航海数据 https://open-live.bilibili.com/doc/2/2/2
    /// </summary>
    [Serializable]
    public struct Guard
    {
        /// <summary>
        /// 大航海等级
        /// </summary>r
        [JsonProperty("guard_level")] public long guardLevel;

        /// <summary>
        /// 大航海数量
        /// </summary>
        [JsonProperty("guard_num")] public long guardNum;

        /// <summary>
        /// 大航海单位 "月"
        /// </summary>
        [JsonProperty("guard_unit")] public string guardUnit;

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
        /// 赠送大航海的用户数据
        /// </summary>
        [JsonProperty("user_info")] public UserInfo userInfo;

        /// <summary>
        /// 房间号
        /// </summary>
        [JsonProperty("room_id")] public long roomID;
    }
}