using System;
using UnityRawInput;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace VTS_DelayExpression
{
    public class DelayExpressionConfig
    {
        [JsonIgnore]
        public List<RawKey> PressingHotkey = new List<RawKey>();
        public List<string> PressingHotkeys = new List<string>();
        public bool GlobalHotkey;
        public List<PlayPoint> PlayPoints;
    }

    [Serializable]
    public class PlayPoint
    {
        // 播放时间点
        public float PlayTime;
        // 表情名字
        public string Expression;
        // 淡入淡出时间
        public float FadeSecondsAmount;
        // 播放后多长时间淡出
        //public float FadeOutAfter;
    }
}
