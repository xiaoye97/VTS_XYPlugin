using HarmonyLib;
using System.Collections.Generic;

namespace VTS_MutiMotionPlayer
{
    public static class Live2DModelAnimatorPatch
    {
        [HarmonyPrefix, HarmonyPatch(typeof(Live2DModelAnimator), "LateUpdate")]
        public static bool Live2DModelAnimator_LateUpdate_Patch(Live2DModelAnimator __instance)
        {
            if (__instance.availableVTSModels.Count == 0)
            {
                return false;
            }
            __instance.trackingLostBehaviour = ConfigTab_Camera.GetTrackingLostBehaviour();
            __instance.step_DoFaceLostBlend();
            __instance.step_SetValuesToDefault();
            step_SetValuesFromAnimation(__instance);
            __instance.step_SetValuesFromExpression();
            __instance.step_DoInputPreProcessing();
            __instance.step_SetValuesFromFaceTracking();
            if (PlatformHelper.IsDesktop && MultiSelectionWindow.ModelCurrentlyFrozen())
            {
                return false;
            }
            __instance.step_DoModelPositionMovement();
            foreach (VTubeStudioModel vtubeStudioModel in __instance.availableVTSModels.Keys)
            {
                // 这里是主ParamStateController的刷新等操作
                vtubeStudioModel.ParamStateController.ExecuteStateStep();
                bool flag = __instance.paramOverride.OverridesActive();
                foreach (ModelParamState modelParamState in vtubeStudioModel.ParamStateController.ParamStates)
                {
                    if (!flag)
                    {
                        vtubeStudioModel.SetLive2DParam(modelParamState.Parameter, modelParamState.Value);
                    }
                    else if (flag)
                    {
                        if (!__instance.paramOverride.HasOverride(modelParamState.Parameter))
                        {
                            vtubeStudioModel.SetLive2DParam(modelParamState.Parameter, modelParamState.Parameter.DefaultValue);
                        }
                        else
                        {
                            vtubeStudioModel.SetLive2DParam(modelParamState.Parameter, __instance.paramOverride.GetOverride(modelParamState.Parameter));
                        }
                    }
                }
                // 紧接着刷新自己的ParamStateController
                foreach (var psc in MutiMotionPlayer.NowModelMutiPSCs)
                {
                    psc.ExecuteStateStep();
                    foreach (ModelParamState modelParamState in psc.ParamStates)
                    {
                        if (!flag)
                        {
                            vtubeStudioModel.SetLive2DParam(modelParamState.Parameter, modelParamState.Value);
                        }
                        else if (flag)
                        {
                            if (!__instance.paramOverride.HasOverride(modelParamState.Parameter))
                            {
                                vtubeStudioModel.SetLive2DParam(modelParamState.Parameter, modelParamState.Parameter.DefaultValue);
                            }
                            else
                            {
                                vtubeStudioModel.SetLive2DParam(modelParamState.Parameter, __instance.paramOverride.GetOverride(modelParamState.Parameter));
                            }
                        }
                    }
                }
            }
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Live2DModelAnimator), "PlayAnimation")]
        public static bool Live2DModelAnimator_PlayAnimation_Patch(Live2DModelAnimator __instance, VTubeStudioModel vtsModel, Live2DAnimation newAnimation, Live2DAnimationType type, bool stopsOnLastFrame, float fadeOutAfterHotkey)
        {
            if (vtsModel == null)
            {
                return false;
            }
            if (newAnimation == null)
            {
                vtsModel.AnimationMixer.StopAnimation(type);
                return false;
            }
            // 检查要播放动画的模型是否是当前插件控制的模型，如果不是则重新加载配置文件
            if (MutiMotionPlayer.NowControlModel != vtsModel)
            {
                MutiMotionPlayer.NowControlModel = vtsModel;
                MutiMotionPlayer.LoadConfig();
            }
            // 检查当前动画有没有配置MixerID
            MutiMotionConfig targetConfig = null;
            foreach (var config in MutiMotionPlayer.configs)
            {
                if (config.ControlMotions.Contains(newAnimation.name))
                {
                    targetConfig = config;
                    break;
                }
            }
            var mixer = vtsModel.AnimationMixer;
            var psc = vtsModel.ParamStateController;
            // 如果动画有配置混合器ID的话，则使用子混合器
            if (targetConfig != null)
            {
                mixer = MutiMotionPlayer.NowModelMutiMixers[targetConfig.MutiPlayerID];
                psc = MutiMotionPlayer.NowModelMutiPSCs[targetConfig.MutiPlayerID];
            }
            if (type == Live2DAnimationType.IdleAnimation)
            {
                if (targetConfig != null)
                {
                    Live2DAnimationMixerPatch.StartIdleAnimation(mixer, psc, newAnimation);
                }
                else
                {
                    mixer.StartIdleAnimation(newAnimation);
                }
                return false;
            }
            if (type == Live2DAnimationType.OneShotAnimation)
            {
                if (targetConfig != null)
                {
                    Live2DAnimationMixerPatch.StartNormalAnimation(mixer, psc, newAnimation, stopsOnLastFrame, fadeOutAfterHotkey);
                }
                else
                {
                    mixer.StartNormalAnimation(newAnimation, stopsOnLastFrame, fadeOutAfterHotkey);
                }
            }
            return false;
        }

        public static void step_SetValuesFromAnimation(Live2DModelAnimator __instance)
        {
            foreach (KeyValuePair<VTubeStudioModel, LastKnownLive2DParamValues> keyValuePair in __instance.availableVTSModels)
            {
                keyValuePair.Key.AnimationMixer.StepAnimationCurves(keyValuePair.Value.lastKnownParamValueOrDefault);
                for (int i = 0; i < MutiMotionPlayer.NowModelMutiMixers.Count; i++)
                {
                    Live2DAnimationMixerPatch.StepAnimationCurves(MutiMotionPlayer.NowModelMutiMixers[i], MutiMotionPlayer.NowModelMutiPSCs[i], keyValuePair.Value.lastKnownParamValueOrDefault);
                }
            }
        }
    }
}