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

namespace VTS_ParameterTriggerAudio
{
    public class ParameterTriggerAudioBehaviour : XYCustomBehaviour
    {
        public static ParameterTriggerAudioBehaviour Inst;
        private bool inited;
        public List<ParameterTriggerConfig> configs = new List<ParameterTriggerConfig>();
        public DirectoryInfo AudioFolder;
        public static Dictionary<string, AudioClip> Audios = new Dictionary<string, AudioClip>();
        public List<AudioPlayer> PlayerList = new List<AudioPlayer>();

        public void Start()
        {
            Inst = this;
            if (XYModelManager.Instance.NowModelDef != null)
            {
                Init(XYModelManager.Instance.NowModelDef);
            }
            else
            {
                XYLog.LogMessage("当前没有模型，ParameterTriggerAudio不进行初始化");
            }
        }

        public void Update()
        {
            if (!inited) return;
            if (XYModelManager.Instance.NowModel == null) return;
            foreach (var player in PlayerList)
            {
                if (player.CanUse)
                {
                    // 只有在有音频的情况下才进行检查
                    if (Audios.ContainsKey(player.Config.AudioFile))
                    {
                        player.CheckParameter();
                    }
                }
            }
        }

        public void OnDestroy()
        {
            Clear();
        }

        public void Clear()
        {
            for (int i = 0; i < PlayerList.Count; i++)
            {
                if (PlayerList[i].LoopAudioSource != null)
                {
                    LeanPool.Despawn(PlayerList[i].LoopAudioSource.gameObject);
                }
            }
            PlayerList.Clear();
        }

        public void Init(ModelDefinitionJSON modelDef)
        {
            LoadConfig(modelDef);
            inited = true;
        }

        public void LoadConfig(ModelDefinitionJSON modelDef)
        {
            Clear();
            configs = new List<ParameterTriggerConfig>();
            string path = modelDef.FilePath.Replace(".vtube.json", ".ParameterTriggerAudio.json");
            FileInfo file = new FileInfo(path);
            AudioFolder = new DirectoryInfo(file.Directory.FullName);
            if (file.Exists)
            {
                string json = FileHelper.ReadAllText(file.FullName);
                try
                {
                    configs = JsonConvert.DeserializeObject<List<ParameterTriggerConfig>>(json);
                }
                catch (Exception ex)
                {
                    XYLog.LogError($"ParameterTriggerAudio解析配置文件异常 {ex}");
                }
            }
            else
            {
                //ParameterTriggerConfig config = new ParameterTriggerConfig();
                //config.AudioFile = "Audios/Click.wav";
                //config.Loop = false;
                //config.Volume = 1;
                //config.Parameters.Add("glasses");
                //config.Operations.Add("=");
                //config.Values.Add(30);
                //configs.Add(config);
                //var json = JsonConvert.SerializeObject(configs, Formatting.Indented);
                //FileHelper.WriteAllText(path, json);
            }
            for (int i = 0; i < configs.Count; i++)
            {
                AudioPlayer player = new AudioPlayer(configs[i]);
                player.Init();
                PlayerList.Add(player);
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
                XYLog.LogMessage($"ParameterTriggerAudio加载了音频:{audioPath}");
            }
            else
            {
                XYLog.LogWarning($"加载音频 {audioPath} 出现异常，{_unityWebRequest.error}");
            }
            yield break;
        }
    }
}