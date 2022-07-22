using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace VTS_MutiMotionPlayer
{
    public static class Live2DModelAnimatorPatch
    {
        [HarmonyPrefix, HarmonyPatch(typeof(Live2DModelAnimator), "LateUpdate")]
        public static bool Live2DModelAnimator_LateUpdate_Patch(Live2DModelAnimator __instance)
        {
            __instance.modelCount = __instance.availableVTSModels.Count;
            if (__instance.modelCount == 0)
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
                ModelDefinitionJSON modelJSON = vtubeStudioModel.ModelJSON;
                bool flag = vtubeStudioModel != __instance.mainModel;
                Live2DItemShift live2DItemShift = null;
                float t = 1f;
                if (flag)
                {
                    bool flag2 = !modelJSON.ItemSettings.OnlyMoveWhenPinned || (modelJSON.ItemSettings.OnlyMoveWhenPinned && modelJSON.ItemSettings.ItemInfo != null && modelJSON.ItemSettings.ItemInfo.IsPinned);
                    modelJSON.ItemSettings.MovementFade = (modelJSON.ItemSettings.MovementFade + Time.deltaTime * 3.8f * (float)(flag2 ? 1 : -1)).Clamp01();
                    t = modelJSON.ItemSettings.MovementFade;
                    live2DItemShift = vtubeStudioModel.ItemShiftController;
                }

                vtubeStudioModel.ParamStateController.ExecuteStateStep();
                bool flag3 = __instance.paramOverride.OverridesActive();
                foreach (ModelParamState modelParamState in vtubeStudioModel.ParamStateController.ParamStates)
                {
                    Live2DParamNames.Live2DParameterID paramID = Live2DParamNames.Live2DParameterID.NonDefault;
                    float num = 1f;
                    if (flag)
                    {
                        bool flag4;
                        paramID = __instance.step_GetItemMultiplierForParameter(modelParamState.Parameter, modelJSON, out num, out flag4);
                        if (flag4)
                        {
                            num = Mathf.Lerp(0f, num, t);
                        }
                    }
                    if (!flag3)
                    {
                        vtubeStudioModel.SetLive2DParam(modelParamState.Parameter, modelParamState.Value * num);
                        if (live2DItemShift != null)
                        {
                            live2DItemShift.SetParam(paramID, modelParamState.Value);
                        }
                    }
                    else if (flag3)
                    {
                        if (!__instance.paramOverride.HasOverride(modelParamState.Parameter))
                        {
                            vtubeStudioModel.SetLive2DParam(modelParamState.Parameter, modelParamState.Parameter.DefaultValue * num);
                            if (live2DItemShift != null)
                            {
                                live2DItemShift.SetParam(paramID, modelParamState.Parameter.DefaultValue);
                            }
                        }
                        else
                        {
                            float @override = __instance.paramOverride.GetOverride(modelParamState.Parameter);
                            vtubeStudioModel.SetLive2DParam(modelParamState.Parameter, @override);
                            if (live2DItemShift != null)
                            {
                                live2DItemShift.SetParam(paramID, @override);
                            }
                        }
                    }
                }
                // 紧接着刷新自己的ParamStateController
                foreach (var psc in MutiMotionPlayer.NowModelMutiPSCs)
                {
                    psc.ExecuteStateStep();
                    foreach (ModelParamState modelParamState in vtubeStudioModel.ParamStateController.ParamStates)
                    {
                        Live2DParamNames.Live2DParameterID paramID = Live2DParamNames.Live2DParameterID.NonDefault;
                        float num = 1f;
                        if (flag)
                        {
                            bool flag4;
                            paramID = __instance.step_GetItemMultiplierForParameter(modelParamState.Parameter, modelJSON, out num, out flag4);
                            if (flag4)
                            {
                                num = Mathf.Lerp(0f, num, t);
                            }
                        }
                        if (!flag3)
                        {
                            vtubeStudioModel.SetLive2DParam(modelParamState.Parameter, modelParamState.Value * num);
                            if (live2DItemShift != null)
                            {
                                live2DItemShift.SetParam(paramID, modelParamState.Value);
                            }
                        }
                        else if (flag3)
                        {
                            if (!__instance.paramOverride.HasOverride(modelParamState.Parameter))
                            {
                                vtubeStudioModel.SetLive2DParam(modelParamState.Parameter, modelParamState.Parameter.DefaultValue * num);
                                if (live2DItemShift != null)
                                {
                                    live2DItemShift.SetParam(paramID, modelParamState.Parameter.DefaultValue);
                                }
                            }
                            else
                            {
                                float @override = __instance.paramOverride.GetOverride(modelParamState.Parameter);
                                vtubeStudioModel.SetLive2DParam(modelParamState.Parameter, @override);
                                if (live2DItemShift != null)
                                {
                                    live2DItemShift.SetParam(paramID, @override);
                                }
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