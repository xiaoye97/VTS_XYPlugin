using BepInEx;
using UnityEngine;
using VTS_XYPlugin_Common;

namespace VTS_ParameterTriggerAudio
{
    [BepInDependency("me.xiaoye97.plugin.VTubeStudio.VTS_XYPlugin", "2.0.0")]
    [BepInPlugin(GUID, PluginName, VERSION)]
    [ExScript(PluginName, PluginDescription, "宵夜97", VERSION)]
    public class ParameterTriggerAudio : BaseUnityPlugin
    {
        public const string GUID = "me.xiaoye97.plugin.VTubeStudio.ParameterTriggerAudio";
        public const string PluginName = "ParameterTriggerAudio[参数触发音效]";
        public const string PluginDescription = "当模型的某个参数满足设定范围时，触发对应的音效。[需要在配置文件中设置相关数据][音频文件支持wav ogg aac三种格式]";
        public const string VERSION = "1.0.0";
        public static GameObject AudioSourcePrefab;

        private void Start()
        {
            CreateAudioSourcePrefab();
        }

        public void CreateAudioSourcePrefab()
        {
            AudioSourcePrefab = new GameObject("AudioSourcePrefab");
            var audioSource = AudioSourcePrefab.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            var auto = AudioSourcePrefab.AddComponent<AudioSourceAutoDestroy>();
            auto.audioSource = audioSource;
        }
    }
}