using System;
using Newtonsoft.Json;

namespace OpenBLive.Runtime.Data
{
    /// <summary>
    /// 赠送大航海的用户数据 https://open-live.bilibili.com/doc/2/2/2
    /// </summary>
    [Serializable]
    public struct UserInfo
    {
        /// <summary>
        /// 购买大航海的用户UID
        /// </summary>
        [JsonProperty("uid")] public long uid;

        /// <summary>
        /// 购买大航海的用户昵称
        /// </summary>
        [JsonProperty("uname")] public string userName;

        /// <summary>
        /// 购买大航海的用户头像
        /// </summary>
        [JsonProperty("uface")] public string userFace;
    }
}