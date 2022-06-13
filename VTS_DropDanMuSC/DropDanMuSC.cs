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
using UnityEngine.UI;

namespace VTS_DropDanMuSC
{
    [BepInDependency("me.xiaoye97.plugin.VTubeStudio.VTS_XYPlugin", "2.0.0")]
    [BepInPlugin(GUID, PluginName, VERSION)]
    [ExScript(PluginName, PluginDescription, "宵夜97", VERSION)]
    public class DropDanMuSC : BaseUnityPlugin
    {
        public const string GUID = "me.xiaoye97.plugin.VTubeStudio.DropDanMuSC";
        public const string PluginName = "DropDanMuSC[掉落弹幕和SC]";
        public const string PluginDescription = "让直播间的弹幕和SC像礼物一样掉下来。[需要在配置文件中设置相关数据]";
        public const string VERSION = "1.0.0";
        public static DropDanMuSCConfig config;
        public static DirectoryInfo DropDanMuSCFolder;

        public static AssetBundle AB;
        public static GameObject CanvasPrefab;

        private void Start()
        {
            DropDanMuSCFolder = new DirectoryInfo($"{XYPaths.XYDirPath}/DropDanMuSC");
            MessageCenter.Instance.Register<BDanMuMessage>(OnDanMuRecv);
            MessageCenter.Instance.Register<BSCMessage>(OnSCRecv);
            LoadConfig();
            LoadAB();
        }

        public void OnDanMuRecv(object obj)
        {
            BDanMuMessage message = (BDanMuMessage)obj;
            
        }

        public void OnSCRecv(object obj)
        {
            BSCMessage message = (BSCMessage)obj;

        }

        public void LoadConfig()
        {
            FileInfo file = new FileInfo($"{DropDanMuSCFolder}/DropDanMuSC.json");
            if (file.Exists)
            {
                string json = FileHelper.ReadAllText(file.FullName);
                try
                {
                    config = JsonConvert.DeserializeObject<DropDanMuSCConfig>(json);
                }
                catch (Exception ex)
                {
                    XYLog.LogError($"DropDanMuSC解析配置文件异常 {ex}");
                }
            }
            else
            {
                config = new DropDanMuSCConfig();
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                FileHelper.WriteAllText(file.FullName, json);
            }
        }

        public void LoadAB()
        {
            try
            {
                AB = AssetBundle.LoadFromFile($"{DropDanMuSCFolder.FullName}/{config.ABPath}");
                CanvasPrefab = AB.LoadAsset<GameObject>("DropDanMuSCCanvas");
                var go = GameObject.Instantiate(CanvasPrefab);
                //var camera = GameObject.Find("UI Front Camera").GetComponent<Camera>();
                var camera = GameObject.Find("Live2D Camera").GetComponent<Camera>();
                var canvas = go.GetComponent<Canvas>();
                canvas.worldCamera = camera;
                canvas.sortingOrder = 30000;
                go.transform.GetChild(0).gameObject.AddComponent<RectToBoxCollider2D>();

            }
            catch (Exception ex)
            {
                XYLog.LogError($"{ex}");
            }
        }
    }
}