using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTS_DanMuTriggerAudio
{
    public class DanMuTriggerAudioConfig
    {
        // 弹幕触发词
        public string DanMu;
        // 音频文件路径(相对于VTube Studio\XYPluginConfig\DanMuTriggerAudio)
        public string AudioFile;
        // 多次触发音效是否生成多个声音，如果此值为false，则此音效同时只会存在一个，后面的需要等前面的播放完毕才会再次触发
        public bool Muti = true;
        // 音效的音量，此值介于0-1之间
        public float Volume = 1f;
    }
}
