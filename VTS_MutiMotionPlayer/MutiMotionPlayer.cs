using BepInEx;
using HarmonyLib;
using Live2D.Cubism.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using VTS_XYPlugin;
using VTS_XYPlugin_Common;

namespace VTS_MutiMotionPlayer
{
    [BepInDependency("me.xiaoye97.plugin.VTubeStudio.VTS_XYPlugin", "2.0.0")]
    [BepInPlugin(GUID, PluginName, VERSION)]
    [ExScript(PluginName, PluginDescrition, "宵夜97", VERSION)]
    public class MutiMotionPlayer : BaseUnityPlugin
    {
        public const string GUID = "me.xiaoye97.plugin.VTubeStudio.MutiMotionPlayer";
        public const string PluginName = "MutiMotionPlayer[多轨动画播放]";
        public const string VERSION = "1.0.0";
        public const string PluginDescrition = "让VTS支持同时播放多个普通动画和多个idle动画。[需要在配置文件中设置相关数据]警告！此脚本改动了VTS较多底层逻辑，容易因为VTS更新而失效。请注意备份好VTS，以免因VTS更新而导致无法正常直播。";
        public static List<MutiMotionConfig> configs = new List<MutiMotionConfig>();

        public static VTubeStudioModel NowControlModel;
        public static List<Live2DAnimationMixer> NowModelMutiMixers = new List<Live2DAnimationMixer>();
        public static List<ModelParamStateController> NowModelMutiPSCs = new List<ModelParamStateController>();
        public static List<string> NowControlMotionNames = new List<string>();

        private void Start()
        {
            var modelLoader = GameObject.FindObjectOfType<VTubeStudioModelLoader>();
            if (modelLoader == null)
            {
                XYLog.LogError("未找到模型加载器");
                return;
            }
            modelLoader.modelLoadingFinished.AddListener(OnModelLoadingFinished);
            Harmony.CreateAndPatchAll(typeof(MutiMotionPlayer));
            Harmony.CreateAndPatchAll(typeof(Live2DAnimationMixerPatch));
            Harmony.CreateAndPatchAll(typeof(Live2DModelAnimatorPatch));
            Harmony.CreateAndPatchAll(typeof(MiscPatch));
            XYRawKeyInput.Instance.CheckInputAction += MutiMotionCheckInput;
        }

        public void MutiMotionCheckInput()
        {
            for (int i = 0; i < NowModelMutiMixers.Count; i++)
            {
                var mixer = NowModelMutiMixers[i];
                if (mixer != null)
                {
                    // 如果混合器当前有动画，则检测是否需要关闭动画
                    if (mixer.currentAnimation != null || mixer.currentIdleAnimation != null)
                    {
                        var config = configs[i];
                        var hotkeys = config.StopHotkey;
                        bool needStop = true;
                        foreach (var hotkey in hotkeys)
                        {
                            bool pressed = XYRawKeyInput.GetKey(hotkey) && (config.GlobalHotkey || FocusHelper.AppHasFocus);
                            if (!pressed)
                            {
                                needStop = false;
                                break;
                            }
                        }
                        if (needStop)
                        {
                            var psc = NowModelMutiPSCs[i];
                            if (mixer.currentAnimation != null)
                            {
                                XYLog.LogMessage($"停止播放动画 {mixer.currentAnimation.name}，所在轨道 {i}");
                                Live2DAnimationMixerPatch.StopAnimation(mixer, psc, Live2DAnimationType.OneShotAnimation);
                            }
                            else if (mixer.currentIdleAnimation != null)
                            {
                                XYLog.LogMessage($"停止播放动画 {mixer.currentIdleAnimation.name}，所在轨道 {i}");
                                Live2DAnimationMixerPatch.StopAnimation(mixer, psc, Live2DAnimationType.IdleAnimation);
                            }
                        }
                    }
                }
            }
        }

        private void OnModelLoadingFinished(VTubeStudioModel model)
        {
            MutiMotionPlayer.NowControlModel = model;
            LoadConfig();
        }

        public static void MutiPSCInitialize(ModelParamStateController psc, int mutiID, VTubeStudioModel vtsModel, CubismModel live2DModel)
        {
            psc.vtsModel = vtsModel;
            psc.modelConfigTab = GameObject.FindObjectOfType<ConfigTab_VTSModel>();
            psc.modelConfigTab.currentModelConfigUpdatedEvent.AddListener(new UnityAction(psc.ModelConfigWasUpdated));
            List<string> canControlParams = new List<string>();
            // 根据ID查找配置中所有可控制参数
            var config = configs[mutiID];
            foreach (var p in config.ControlParameters)
            {
                if (!canControlParams.Contains(p))
                {
                    canControlParams.Add(p);
                }
            }
            foreach (CubismParameter cubismParameter in live2DModel.Parameters)
            {
                // 如果此参数没有配进配置文件，则忽略
                if (!canControlParams.Contains(cubismParameter.Id)) continue;
                ModelParamState modelParamState = new ModelParamState();
                modelParamState.Name = cubismParameter.Id;
                modelParamState.Parameter = cubismParameter;
                modelParamState.Weight[ModelParamStateController.IntPrio.prio_0_default] = 1f;
                modelParamState.Targets[ModelParamStateController.IntPrio.prio_0_default] = cubismParameter.DefaultValue;
                modelParamState.Active[ModelParamStateController.IntPrio.prio_0_default] = true;
                modelParamState.Value = cubismParameter.DefaultValue;
                modelParamState.Targets[ModelParamStateController.IntPrio.prio_2_tracking] = cubismParameter.DefaultValue;
                psc.ParamStates.Add(modelParamState);
                psc.ParamStateDictionary.Add(cubismParameter, modelParamState);
            }
            foreach (CubismPart key in live2DModel.Parts)
            {
                psc.cubismPartStates.Add(key, new ModelPartState(1, -1, 0));
            }
            psc.ModelConfigWasUpdated();
        }

        public static void LoadConfig()
        {
            configs = new List<MutiMotionConfig>();
            NowModelMutiMixers = new List<Live2DAnimationMixer>();
            NowModelMutiPSCs = new List<ModelParamStateController>();
            NowControlMotionNames = new List<string>();
            if (XYModelManager.Instance.NowModelDef == null) return;
            string path = XYModelManager.Instance.NowModelDef.FilePath.Replace(".vtube.json", ".MutiMotionPlayer.json");
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                string json = FileHelper.ReadAllText(file.FullName);
                try
                {
                    configs = JsonConvert.DeserializeObject<List<MutiMotionConfig>>(json);
                    // 分配ID
                    for (int i = 0; i < configs.Count; i++)
                    {
                        var config = configs[i];
                        config.MutiPlayerID = i;

                        // 解析快捷键
                        config.StopHotkey = XYRawKeyInput.StringListToRawKeyList(config.StopHotkeys);
                        // 初始化动画混合器
                        var mixer = XYModelManager.Instance.NowModel.gameObject.AddComponent<Live2DAnimationMixer>();
                        mixer.InitializeForModel(XYModelManager.Instance.NowModel);
                        NowModelMutiMixers.Add(mixer);
                        // 初始化参数控制器
                        var psc = XYModelManager.Instance.NowModel.gameObject.AddComponent<ModelParamStateController>();
                        NowModelMutiPSCs.Add(psc);
                        MutiPSCInitialize(psc, i, XYModelManager.Instance.NowModel, XYModelManager.Instance.NowModel.Live2DModel);
                        // 记录所有控制的动画
                        for (int j = 0; j < config.ControlMotions.Count; j++)
                        {
                            string motionName = config.ControlMotions[j];
                            if (!NowControlMotionNames.Contains(motionName))
                            {
                                NowControlMotionNames.Add(motionName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    XYLog.LogError($"MutiMotionPlayer解析配置文件异常 {ex}");
                }
            }
            else
            {
                //MutiMotionConfig config = new MutiMotionConfig();
                //config.StopHotkey.Add(RawKey.Control);
                //config.StopHotkey.Add(RawKey.J);
                //config.StopHotkeys.Add("Control");
                //config.StopHotkeys.Add("J");
                //config.ControlParameters.Add("TestYuanXing");
                //config.ControlMotions.Add("YuanScene.motion3.json");
                //configs.Add(config);
                //var json = JsonConvert.SerializeObject(configs, Formatting.Indented);
                //FileHelper.WriteAllText(path, json);
            }
        }
    }
}