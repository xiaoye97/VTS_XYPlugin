using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using VTS_XYPlugin;
using VTS_XYPlugin_Common;

namespace VTS_DelayInputParameter
{
    [BepInDependency("me.xiaoye97.plugin.VTubeStudio.VTS_XYPlugin", "2.0.0")]
    [BepInPlugin(GUID, PluginName, VERSION)]
    [ExScript(PluginName, PluginDescription, "宵夜97", VERSION)]
    public class DelayInputParameter : BaseUnityPlugin
    {
        public const string GUID = "me.xiaoye97.plugin.VTubeStudio.DelayInputParameter";
        public const string PluginName = "DelayInputParameter[延迟面捕输入参数]";
        public const string PluginDescription = "将VTS的内置面捕输入参数延迟一定的时间再传输到输出参数。[需要在配置文件中设置相关数据][优先级低于使用API的面捕输入(如VBridger)，如果使用此类插件，可以删除此扩展]";
        public const string VERSION = "1.0.0";
        public static List<DelayInputParameterConfig> configs = new List<DelayInputParameterConfig>();
        private string nowControlModel = "";

        private void Start()
        {
            Harmony.CreateAndPatchAll(typeof(DelayInputParameter));
        }

        private void Update()
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
        }

        public static Dictionary<string, Queue<float>> DelayDict = new Dictionary<string, Queue<float>>();
        public static Dictionary<string, int> DelayCountDict = new Dictionary<string, int>();

        [HarmonyPrefix, HarmonyPatch(typeof(Live2DModelAnimator), "step_SetValuesFromFaceTracking")]
        public static bool Live2DModelAnimator_step_SetValuesFromFaceTracking_Patch(Live2DModelAnimator __instance)
        {
            foreach (KeyValuePair<VTubeStudioModel, LastKnownLive2DParamValues> availableVtsModel in __instance.availableVTSModels)
            {
                foreach (ParameterSetting parameterSetting in availableVtsModel.Key.ModelJSON.ParameterSettings)
                {
                    FaceTrackingParamInfo.FaceTrackingParam parameterName = FaceTrackingParamInfo.GetParam(parameterSetting.Input);
                    float faceValue = 0.0f;
                    int num2 = __instance.faceParameters == null ? 0 : (__instance.faceParameters.TryGetFaceTrackingParameterValue(parameterName, out faceValue) ? 1 : 0);
                    bool hasSpecialParam = false;
                    if (num2 == 0)
                        hasSpecialParam = __instance.specialParameters != null && __instance.specialParameters.TryGetFaceSpecialParameterValue(parameterName, out faceValue);

                    // 将当前的数据加入延迟列表
                    if (DelayCountDict.ContainsKey(parameterSetting.Input))
                    {
                        DelayDict[parameterSetting.Input].Enqueue(faceValue);
                        if (DelayDict[parameterSetting.Input].Count > DelayCountDict[parameterSetting.Input])
                        {
                            faceValue = DelayDict[parameterSetting.Input].Dequeue();
                        }
                    }

                    APITrackingData currentTrackingData = Executor_InjectParameterDataRequest.GetCurrentTrackingData();
                    string lower = parameterSetting.Input.ToLower();
                    bool hasAPIParam = false;
                    if (currentTrackingData.apiTrackingParams.ContainsKey(lower))
                    {
                        APITrackingDataParam apiTrackingParam = currentTrackingData.apiTrackingParams[lower];
                        if (apiTrackingParam.IsCustom)
                        {
                            CustomParam customParam = Executor_ParameterCreationRequest.TryGetCustomParam(lower);
                            faceValue = (float)((double)apiTrackingParam.Weight * (double)apiTrackingParam.Value + (1.0 - (double)apiTrackingParam.Weight) * (double)customParam.DefaultValue);
                        }
                        else
                            faceValue = (float)((double)apiTrackingParam.Weight * (double)apiTrackingParam.Value + (1.0 - (double)apiTrackingParam.Weight) * (double)faceValue);
                        hasAPIParam = true;
                    }
                    bool flag3 = false;
                    if (num2 == 0 && !hasSpecialParam && !hasAPIParam)
                    {
                        CustomParam customParam = Executor_ParameterCreationRequest.TryGetCustomParam(lower);
                        if (customParam != null)
                        {
                            faceValue = customParam.DefaultValue;
                            flag3 = true;
                        }
                    }
                    if ((num2 | (hasSpecialParam ? 1 : 0) | (hasAPIParam ? 1 : 0) | (flag3 ? 1 : 0)) != 0)
                    {
                        if (parameterSetting.ClampInput)
                            faceValue = faceValue.ClampBetween(parameterSetting.InputRangeLower, parameterSetting.InputRangeUpper);
                        faceValue = faceValue.Map(parameterSetting.InputRangeLower, parameterSetting.InputRangeUpper, parameterSetting.OutputRangeLower, parameterSetting.OutputRangeUpper);
                        if (parameterSetting.ClampOutput)
                            faceValue = faceValue.ClampBetween(parameterSetting.OutputRangeLower, parameterSetting.OutputRangeUpper);
                        if (parameterSetting.OutputLive2D.IsNotNullOrEmpty())
                            __instance.calculateFaceTrackingValueForParam(availableVtsModel, parameterSetting, faceValue, isSpecial: (hasSpecialParam | hasAPIParam | flag3));
                    }
                    else if (parameterSetting.UseBreathing)
                        __instance.calculateFaceTrackingValueForParam(availableVtsModel, parameterSetting, faceValue);
                    else if (parameterSetting.UseBlinking)
                        __instance.calculateFaceTrackingValueForParam(availableVtsModel, parameterSetting, parameterSetting.OutputRangeUpper);
                }
            }
            return false;
        }

        public void LoadConfig()
        {
            configs = new List<DelayInputParameterConfig>();
            DelayDict = new Dictionary<string, Queue<float>>();
            DelayCountDict = new Dictionary<string, int>();
            string path = XYModelManager.Instance.NowModelDef.FilePath.Replace(".vtube.json", ".DelayInputParameter.json");
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                string json = FileHelper.ReadAllText(file.FullName);
                try
                {
                    configs = JsonConvert.DeserializeObject<List<DelayInputParameterConfig>>(json);
                    for (int i = 0; i < configs.Count; i++)
                    {
                        var config = configs[i];
                        DelayDict[config.Parameter] = new Queue<float>();
                        DelayCountDict[config.Parameter] = (int)(config.DelayTime * 0.06f);
                        UnityEngine.Debug.Log($"输入参数:{config.Parameter} 延迟时间:{config.DelayTime}ms 延迟个数:{DelayCountDict[config.Parameter]}");
                    }
                }
                catch (Exception ex)
                {
                    XYLog.LogError($"DelayInputParameterConfig解析配置文件异常 {ex}");
                }
            }
            else
            {
                //DelayInputParameterConfig config = new DelayInputParameterConfig();
                //config.Parameter = "Test";
                //config.DelayTime = 500;
                //configs.Add(config);
                //var json = JsonConvert.SerializeObject(configs, Formatting.Indented);
                //FileHelper.WriteAllText(path, json);
            }
        }
    }
}