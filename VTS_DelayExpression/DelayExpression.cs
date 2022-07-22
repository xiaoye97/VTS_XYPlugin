using BepInEx;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VTS_XYPlugin;
using VTS_XYPlugin_Common;

namespace VTS_DelayExpression
{
    [BepInDependency("me.xiaoye97.plugin.VTubeStudio.VTS_XYPlugin", "2.0.0")]
    [BepInPlugin(GUID, PluginName, VERSION)]
    [ExScript(PluginName, PluginDescription, "宵夜97", VERSION)]
    public class DelayExpression : BaseUnityPlugin
    {
        public const string GUID = "me.xiaoye97.plugin.VTubeStudio.DelayExpression";
        public const string PluginName = "DelayExpression[延迟触发表情]";
        public const string PluginDescription = "按下快捷键后，根据配置的时间轴依次添加表情exp。[需要在配置文件中设置相关数据]";
        public const string VERSION = "1.0.1";
        public List<DelayExpressionConfig> configs = new List<DelayExpressionConfig>();

        private string nowControlModel = "";
        private DelayExpressionConfig needPlayConfig;
        private float playStartTime;
        private int pointIndex;

        private void Start()
        {
            XYRawKeyInput.Instance.CheckInputAction += CheckInput;
        }

        private void Update()
        {
            if (needPlayConfig != null)
            {
                float cha = Time.time - playStartTime;
                if (needPlayConfig.PlayPoints != null)
                {
                    if (pointIndex < needPlayConfig.PlayPoints.Count)
                    {
                        var p = needPlayConfig.PlayPoints[pointIndex];
                        if (cha >= p.PlayTime)
                        {
                            if (p.Expression == "ClearExp")
                            {
                                XYModelManager.Instance.NowModel.ExpressionMixer.RemoveAllExpressions();
                            }
                            else
                            {
                                Live2DExpression live2DExpression;
                                if (XYModelManager.Instance.NowModel.Expressions.TryGetValue(p.Expression, out live2DExpression))
                                {
                                    live2DExpression.fadeSpeed = p.FadeSecondsAmount;
                                    XYModelManager.Instance.NowModel.ExpressionMixer.AddExpression(live2DExpression);
                                }
                            }
                            pointIndex++;
                        }
                    }
                    else
                    {
                        needPlayConfig = null;
                    }
                }
                else
                {
                    needPlayConfig = null;
                }
            }
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
                if (XYModelManager.Instance.NowModelDef != null)
                {
                    nowControlModel = XYModelManager.Instance.NowModelDef.Name;
                    LoadConfig();
                }
            }
            foreach (var config in configs)
            {
                if (config.PressingHotkey.Count == 0)
                {
                    continue;
                }
                bool canPlay = true;
                foreach (var hotkey in config.PressingHotkey)
                {
                    bool pressed = XYRawKeyInput.GetKey(hotkey) && (config.GlobalHotkey || FocusHelper.AppHasFocus);
                    if (!pressed)
                    {
                        canPlay = false;
                        break;
                    }
                }
                if (canPlay)
                {
                    StartDelayPlay(config);
                }
            }
        }

        /// <summary>
        /// 开始延迟播放播放
        /// </summary>
        /// <param name="config"></param>
        public void StartDelayPlay(DelayExpressionConfig config)
        {
            needPlayConfig = config;
            playStartTime = Time.time;
            pointIndex = 0;
        }

        public void LoadConfig()
        {
            configs = new List<DelayExpressionConfig>();
            string path = XYModelManager.Instance.NowModelDef.FilePath.Replace(".vtube.json", ".DelayExpression.json");
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                string json = FileHelper.ReadAllText(file.FullName);
                try
                {
                    configs = JsonConvert.DeserializeObject<List<DelayExpressionConfig>>(json);
                    for (int i = 0; i < configs.Count; i++)
                    {
                        var config = configs[i];
                        config.PressingHotkey = XYRawKeyInput.StringListToRawKeyList(config.PressingHotkeys);
                    }
                }
                catch (Exception ex)
                {
                    XYLog.LogError($"PressingMotionPlayer解析配置文件异常 {ex}");
                }
            }
            else
            {
                XYLog.LogMessage($"DelayExpression:未找到配置文件 {path}");
                //DelayExpressionConfig config = new DelayExpressionConfig();
                //config.PressingHotkey = RawKey.Numpad4;
                //config.GlobalHotkey = true;
                //config.PlayPoints = new List<PlayPoint>();
                //PlayPoint p1 = new PlayPoint();
                //p1.PlayTime = 1;
                //p1.Expression = "EyesCry.exp3.json";
                //p1.FadeSecondsAmount = 0.5f;
                ////p1.FadeOutAfter = -1;
                //PlayPoint p2 = new PlayPoint();
                //p2.PlayTime = 4;
                //p2.Expression = "EyesLove.exp3.json";
                //p2.FadeSecondsAmount = 0.5f;
                ////p2.FadeOutAfter = -1;
                //config.PlayPoints.Add(p1);
                //config.PlayPoints.Add(p2);
                //configs.Add(config);
                //var json = JsonConvert.SerializeObject(configs, Formatting.Indented);
                //FileHelper.WriteAllText(path, json);
            }
        }
    }
}