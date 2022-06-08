using System.Collections.Generic;

namespace VTS_ParameterTriggerAudio
{
    public class ParameterTriggerConfig
    {
        // 音效文件的相对路径，支持wav ogg aac格式的音频，推荐使用wav，aac可能不稳定，如果无法播放，请转换为wav格式
        public string AudioFile;

        // 音效是否循环播放
        public bool Loop = false;

        // 当音效循环播放时，进入循环和退出循环的淡入淡出过渡时间
        public float LoopFadeTime = 0f;

        // 当音效不循环时，多次触发音效是否生成多个声音，如果此值为false，则此音效同时只会存在一个，后面的需要等前面的播放完毕才会再次触发
        public bool Muti = true;

        // 音效的音量，此值介于0-1之间
        public float Volume = 1f;

        // 触发音效所需的模型参数列表
        public List<string> Parameters = new List<string>();

        // 参数的判断符号列表，支持5种判断 = < > <= >=
        public List<string> Operations = new List<string>();

        // 参数的判断值列表
        public List<float> Values = new List<float>();

        // 参数是否为面捕输入参数列表。如果此处填写true，则会使用面捕输入参数来判断，如果为false，则使用模型上的参数来判断。
        public List<bool> IsInputParam = new List<bool>();
    }
}