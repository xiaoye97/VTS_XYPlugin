using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityRawInput;

namespace VTS_DelayExpression
{
    public class DelayExpressionConfig
    {
        [JsonIgnore]
        public List<RawKey> PressingHotkey = new List<RawKey>();

        // 快捷键组合
        public List<string> PressingHotkeys = new List<string>();

        // 是否为全局快捷键
        public bool GlobalHotkey;

        // 播放点列表
        public List<PlayPoint> PlayPoints;
    }

    [Serializable]
    public class PlayPoint
    {
        // 播放时间点(秒)
        public float PlayTime;

        // 表情文件名
        public string Expression;

        // 淡入淡出时间(秒)
        public float FadeSecondsAmount;
    }
}