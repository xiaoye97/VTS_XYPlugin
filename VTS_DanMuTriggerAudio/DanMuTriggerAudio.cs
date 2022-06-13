using BepInEx;
using Lean.Pool;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using VTS_XYPlugin;
using VTS_XYPlugin_Common;

namespace VTS_DanMuTriggerAudio
{
    [BepInDependency("me.xiaoye97.plugin.VTubeStudio.VTS_XYPlugin", "2.0.0")]
    [BepInPlugin(GUID, PluginName, VERSION)]
    [ExScript(PluginName, PluginDescription, "宵夜97", VERSION)]
    public class DanMuTriggerAudio : BaseUnityPlugin
    {
        public const string GUID = "me.xiaoye97.plugin.VTubeStudio.DanMuTriggerAudio";
        public const string PluginName = "DanMuTriggerAudio[弹幕触发音效]";
        public const string PluginDescription = "当直播间发送的弹幕包含触发词时，触发对应的音效。[需要在配置文件中设置相关数据][音频文件支持wav ogg aac三种格式]";
        public const string VERSION = "1.0.0";
        public static GameObject AudioSourcePrefab;
        public static Dictionary<string, AudioClip> Audios = new Dictionary<string, AudioClip>();
        public static DirectoryInfo AudioFolder;
        public static List<DanMuTriggerAudioConfig> configs = new List<DanMuTriggerAudioConfig>();
        public static Dictionary<string, List<AudioSourceAutoDestroy>> AudioSources = new Dictionary<string, List<AudioSourceAutoDestroy>>();

        private void Start()
        {
            CreateAudioSourcePrefab();
            AudioFolder = new DirectoryInfo($"{XYPaths.XYDirPath}/DanMuTriggerAudio");
            MessageCenter.Instance.Register<BDanMuMessage>(OnDanMuRecv);
            LoadConfig();
        }

        public void OnDanMuRecv(object obj)
        {
            BDanMuMessage message = (BDanMuMessage)obj;
            foreach (var config in configs)
            {
                // 如果包含关键词，则触发音效
                if (message.弹幕.Contains(config.DanMu) && Audios.ContainsKey(config.AudioFile))
                {
                    if (config.Muti || AudioSources[config.AudioFile].Count == 0)
                    {
                        var go = LeanPool.Spawn(AudioSourcePrefab);
                        var com = go.GetComponent<AudioSourceAutoDestroy>();
                        com.config = config;
                        com.audioSource.loop = false;
                        com.autoDestroy = true;
                        com.audioSource.clip = Audios[config.AudioFile];
                        com.audioSource.volume = config.Volume;
                        com.audioSource.Play();
                        XYLog.LogMessage($"弹幕:{message.弹幕}触发了音频:{config.AudioFile}");
                        break;
                    }
                }
            }
        }

        public void CreateAudioSourcePrefab()
        {
            AudioSourcePrefab = new GameObject("DanMuTriggerAudioSourcePrefab");
            var audioSource = AudioSourcePrefab.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            var auto = AudioSourcePrefab.AddComponent<AudioSourceAutoDestroy>();
            auto.audioSource = audioSource;
        }

        public void LoadConfig()
        {
            configs = new List<DanMuTriggerAudioConfig>();
            FileInfo file = new FileInfo($"{AudioFolder}/DanMuTriggerAudio.json");
            if (file.Exists)
            {
                string json = FileHelper.ReadAllText(file.FullName);
                try
                {
                    configs = JsonConvert.DeserializeObject<List<DanMuTriggerAudioConfig>>(json);
                    foreach (var config in configs)
                    {
                        AudioSources[config.AudioFile] = new List<AudioSourceAutoDestroy>();
                        LoadAudio(config.AudioFile);
                    }
                }
                catch (Exception ex)
                {
                    XYLog.LogError($"DanMuTriggerAudio解析配置文件异常 {ex}");
                }
            }
            else
            {
                //DanMuTriggerAudioConfig config = new DanMuTriggerAudioConfig();
                //config.DanMu = "大叫";
                //config.AudioFile = "Audios/大叫.wav";
                //config.Volume = 1;
                //config.Muti = true;
                //configs.Add(config);
                //var json = JsonConvert.SerializeObject(configs, Formatting.Indented);
                //FileHelper.WriteAllText(file.FullName, json);
            }
        }

        public void LoadAudio(string audioPath)
        {
            StartCoroutine(IELoadAudio(audioPath));
        }

        private IEnumerator IELoadAudio(string audioPath)
        {
            string url = $"file://{AudioFolder.FullName}/{audioPath}";
            AudioType audioType = AudioType.UNKNOWN;
            if (audioPath.EndsWith(".wav")) audioType = AudioType.WAV;
            else if (audioPath.EndsWith(".ogg")) audioType = AudioType.OGGVORBIS;
            else if (audioPath.EndsWith(".aac")) audioType = AudioType.ACC;
            else
            {
                // 如果格式不支持，则不加载
                XYLog.LogWarning($"音频文件{audioPath}的格式不受支持，请尝试转换到.wav .ogg .aac格式");
                yield break;
            }

            UnityWebRequest _unityWebRequest = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
            yield return _unityWebRequest.SendWebRequest();
            if (_unityWebRequest.result == UnityWebRequest.Result.Success)
            {
                AudioClip _audioClip = DownloadHandlerAudioClip.GetContent(_unityWebRequest);
                Audios[audioPath] = _audioClip;
                XYLog.LogMessage($"DanMuTriggerAudio加载了音频:{audioPath}");
            }
            else
            {
                XYLog.LogWarning($"加载音频 {audioPath} 出现异常，{_unityWebRequest.error}");
            }
            yield break;
        }
    }
}