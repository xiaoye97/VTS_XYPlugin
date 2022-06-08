using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityRawInput;

namespace VTS_PressingMotionPlayer
{
    public class PressingConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public RawKey PressingHotkey;

        // 是否是全局按键
        public bool GlobalHotkey;

        // 动画的名字
        public string IdleAnimationName;

        // 动画淡入淡出的时间
        public float FadeSecondsAmount;
    }
}