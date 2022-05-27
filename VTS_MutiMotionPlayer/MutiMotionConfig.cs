using System;
using UnityRawInput;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace VTS_MutiMotionPlayer
{
    public class MutiMotionConfig
    {
        [JsonIgnore]
        public int MutiPlayerID;
        [JsonIgnore]
        public List<RawKey> StopHotkey = new List<RawKey>();

        public List<string> ControlParameters = new List<string>();
        public List<string> ControlMotions = new List<string>();
        public List<string> StopHotkeys = new List<string>();
        public bool GlobalHotkey = true;
    }
}
