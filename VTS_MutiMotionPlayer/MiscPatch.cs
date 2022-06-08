using HarmonyLib;
using VTS_XYPlugin;

namespace VTS_MutiMotionPlayer
{
    public static class MiscPatch
    {
        [HarmonyPrefix, HarmonyPatch(typeof(HotkeyManager), "SwitchToIdleAnimation")]
        public static bool HotkeyManager_SwitchToIdleAnimation_Patch(HotkeyManager __instance, Live2DAnimation idleAnimation)
        {
            // 检测动画是否是在多轨上，如果是的话由插件播放，如果不是则跳过
            if (MutiMotionPlayer.NowControlMotionNames.Contains(idleAnimation.name))
            {
                var nowModel = XYModelManager.Instance.NowModel;
                Live2DModelAnimatorPatch.Live2DModelAnimator_PlayAnimation_Patch(nowModel.ModelAnimator, nowModel, idleAnimation, Live2DAnimationType.IdleAnimation, false, -1f);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}