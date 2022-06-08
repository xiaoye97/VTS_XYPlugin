using BepInEx;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using VTS_XYPlugin;
using VTS_XYPlugin_Common;

namespace VTS_PressingMotionPlayer
{
    [BepInDependency("me.xiaoye97.plugin.VTubeStudio.VTS_XYPlugin", "2.0.0")]
    [BepInPlugin(GUID, PluginName, VERSION)]
    [ExScript(PluginName, PluginDescription, "宵夜97", VERSION)]
    public class PressingMotionPlayer : BaseUnityPlugin
    {
        public const string GUID = "me.xiaoye97.plugin.VTubeStudio.PressingMotionPlayer";
        public const string PluginName = "PressingMotionPlayer[按住播放动画]";
        public const string PluginDescription = "按住快捷键时循环播放目标动画，松开时回到原动画。[需要在配置文件中设置相关数据]";
        public const string VERSION = "1.0.0";
        public List<PressingConfig> configs = new List<PressingConfig>();

        private string nowControlModel = "";
        private bool needPlay = false;
        private PressingConfig needPlayConfig;

        private void Start()
        {
            XYRawKeyInput.Instance.CheckInputAction += CheckInput;
        }

        public void CheckInput()
        {
            // 如果模型为空，则清空配置
            if (XYModelManager.Instance.NowModel == null)
            {
                configs = null;
                nowControlModel = "";
                return;
            }
            // 如果当前模型不为空但是配置为空，则创建配置
            if (configs == null || nowControlModel != XYModelManager.Instance.NowModelDef.Name)
            {
                nowControlModel = XYModelManager.Instance.NowModelDef.Name;
                LoadConfig();
            }
            foreach (var config in configs)
            {
                if (XYRawKeyInput.GetKeyDown(config.PressingHotkey))
                {
                    if (config.GlobalHotkey || FocusHelper.AppHasFocus)
                    {
                        // 记录当前待机动画并切换到指定待机动画
                        if (XYModelManager.Instance.NowModel.AnimationMixer.currentIdleAnimation != null)
                        {
                            needPlay = true;
                            needPlayConfig = config;
                        }
                        else
                        {
                            needPlay = false;
                        }
                    }
                }
                if (XYRawKeyInput.GetKeyUp(config.PressingHotkey))
                {
                    if (config.GlobalHotkey || FocusHelper.AppHasFocus)
                    {
                        // 停止动画
                        needPlay = false;
                        needPlayConfig = null;
                        XYModelManager.Instance.NowModel.AnimationMixer.StopAnimation(Live2DAnimationType.OneShotAnimation);
                    }
                }
            }
            if (needPlay)
            {
                if (XYModelManager.Instance.NowModel.ModelAnimator != null)
                {
                    if (needPlayConfig != null)
                    {
                        if (XYModelManager.Instance.NowModel.AnimationMixer.currentAnimation == null)
                        {
                            // 查找目标动画
                            if (XYModelManager.Instance.NowModel.Animations.ContainsKey(needPlayConfig.IdleAnimationName))
                            {
                                var anim = XYModelManager.Instance.NowModel.Animations[needPlayConfig.IdleAnimationName];
                                anim.fadeSpeed = needPlayConfig.FadeSecondsAmount;
                                XYModelManager.Instance.NowModel.AnimationMixer.animationFadeOngoing = false;
                                XYHotkeyManager.Instance.HotkeyManager.PlayOneShotAnimation(anim, false, -1, XYModelManager.Instance.NowModel);
                            }
                        }
                    }
                }
            }
        }

        public void LoadConfig()
        {
            configs = new List<PressingConfig>();
            string path = XYModelManager.Instance.NowModelDef.FilePath.Replace(".vtube.json", ".PressingMotionPlayer.json");
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                string json = FileHelper.ReadAllText(file.FullName);
                try
                {
                    var con = JsonConvert.DeserializeObject<List<PressingConfig>>(json);
                    configs = con;
                }
                catch (Exception ex)
                {
                    XYLog.LogError($"PressingMotionPlayer解析配置文件异常 {ex}");
                }
            }
            else
            {
                //PressingConfig config = new PressingConfig();
                //config.PressingHotkey = RawKey.Numpad7;
                //config.IdleAnimationName = "01.motion3.json";
                //config.GlobalHotkey = true;
                //config.FadeSecondsAmount = 0.5f;
                //configs.Add(config);
                //var json = JsonConvert.SerializeObject(configs, Formatting.Indented);
                //FileHelper.WriteAllText(path, json);
            }
        }
    }
}