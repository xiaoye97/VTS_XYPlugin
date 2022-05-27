using UnityRawInput;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VTS_PressingMotionPlayer
{
    public class PressingConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public RawKey PressingHotkey;
        public bool GlobalHotkey;
        public string IdleAnimationName;
        public float FadeSecondsAmount;
    }
}
