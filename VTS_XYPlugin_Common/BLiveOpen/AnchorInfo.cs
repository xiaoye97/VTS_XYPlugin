using System;
using Newtonsoft.Json;

namespace OpenBLive.Runtime.Data
{
    /// <summary>
    /// 礼物数据中的主播数据 https://open-live.bilibili.com/doc/2/2/1
    /// </summary>
    [Serializable]
    public struct AnchorInfo
    {
        /// <summary>
        /// 收礼主播UID
        /// </summary>
        [JsonProperty("uid")] public long uid;

        /// <summary>
        /// 收礼主播昵称
        /// </summary>
        [JsonProperty("uname")] public string userName;

        /// <summary>
        /// 收礼主播头像
        /// </summary>
        [JsonProperty("uface")] public string userFace;
    }
}