using System;
using BepInEx;
using System.IO;
using HarmonyLib;
using VTS_XYPlugin;
using VTS_XYPlugin_Common;

namespace VTS_TimeParameter
{
    [BepInDependency("me.xiaoye97.plugin.VTubeStudio.VTS_XYPlugin", "2.0.0")]
    [BepInPlugin(GUID, PluginName, VERSION)]
    [ExScript(PluginName, PluginDescription, "宵夜97", VERSION)]
    public class TimeParameter : BaseUnityPlugin
    {
        public const string GUID = "me.xiaoye97.plugin.VTubeStudio.TimeParameter";
        public const string PluginName = "TimeParameter[时间参数]";
        public const string PluginDescription = "将现实时间的年月日时分秒作为参数输入Live2D模型，支持表达式计算。[需要在配置文件中设置相关数据]";
        public const string VERSION = "1.0.0";

        public void Start()
        {
            Harmony.CreateAndPatchAll(typeof(TimeParameter));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Live2DModelAnimator), "LateUpdate")]
        public static void Live2DModelAnimator_LateUpdate_Patch(Live2DModelAnimator __instance)
        {
            if (XYModelManager.Instance.NowModel == null) return;
            if (XYModelManager.Instance.NowModel.ModelAnimator == null) return;
            if (XYModelManager.Instance.NowModel.ModelAnimator != __instance) return;
            
            foreach (var kv in TimeParameterBehaviour.ParameterDict)
            {
                float ori = kv.Key.Value;
                XYModelManager.Instance.NowModel.SetLive2DParam(kv.Key, kv.Value);
                //UnityEngine.Debug.Log($"原值:{ori} 新值:{kv.Value} 实际新值:{kv.Key.Value}");
            }
        }
    }
}
