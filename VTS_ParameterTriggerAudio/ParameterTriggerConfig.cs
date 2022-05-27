using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace VTS_ParameterTriggerAudio
{
    public class ParameterTriggerConfig
    {
        public string AudioFile;
        public bool Loop = false;
        public float LoopFadeTime = 0f; // 过渡时间
        public bool Muti = true; // OneShot是否允许多个同时播放
        public float Volume = 1f; // 0-1
        public List<string> Parameters = new List<string>();
        public List<string> Operations = new List<string>();
        public List<float> Values = new List<float>();
        public List<bool> IsInputParam = new List<bool>();
    }
}
