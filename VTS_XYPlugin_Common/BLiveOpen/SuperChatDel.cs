using System;
using Newtonsoft.Json;

namespace OpenBLive.Runtime.Data
{
    /// <summary>
    /// 付费留言数据下线 https://open-live.bilibili.com/doc/2/2/3
    /// </summary>
    [Serializable]
    public struct SuperChatDel
    {
        /// <summary>
        /// 直播间ID
        /// </summary>
        [JsonProperty("room_id")] public long roomId;

        /// <summary>
        /// 留言id
        /// </summary>
        [JsonProperty("message_ids")] public long[] messageIds;
    }
}