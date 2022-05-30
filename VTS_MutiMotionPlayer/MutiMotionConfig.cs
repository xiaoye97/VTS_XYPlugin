using System;
using UnityRawInput;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace VTS_MutiMotionPlayer
{
    public class MutiMotionConfig
    {
        [JsonIgnore]
        // 轨道ID，同一ID下的动画不能同时播放，请把需要多轨播放的动画分配到不同的ID
        public int MutiPlayerID;
        [JsonIgnore]
        // 停止此轨道动画的快捷键组合
        public List<RawKey> StopHotkey = new List<RawKey>();
        // 此轨道上的动画使用到的参数列表，不同轨道的动画请不要使用相同的参数，避免冲突
        public List<string> ControlParameters = new List<string>();
        // 此轨道上可播放的动画列表
        public List<string> ControlMotions = new List<string>();
        // 停止此轨道动画的快捷键组合
        public List<string> StopHotkeys = new List<string>();
        // 停止播放快捷键是否为全局快捷键
        public bool GlobalHotkey = true;
    }
}
