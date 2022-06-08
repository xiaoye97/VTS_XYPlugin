using Live2D.Cubism.Core;
using System.Collections.Generic;
using UnityEngine;

namespace VTS_MutiMotionPlayer
{
    public static class Live2DAnimationMixerPatch
    {
        public static void StartIdleAnimation(Live2DAnimationMixer mixer, ModelParamStateController psc, Live2DAnimation newIdleAnimation)
        {
            mixer.previousIdleAnimationCurves.Clear();
            mixer.previousIdleAnimationCurves = new Dictionary<string, AnimationCurve>(mixer.idleAnimationCurves);
            mixer.idleAnimationCurves.Clear();
            for (int i = 0; i < newIdleAnimation.data.ParameterIds.Length; i++)
            {
                AnimationCurve value = newIdleAnimation.data.ParameterCurves[i];
                string key = newIdleAnimation.data.ParameterIds[i];
                mixer.idleAnimationCurves.Add(key, value);
            }
            mixer.previousIdleAnimationProgress = mixer.idleAnimationTimer;
            mixer.idleAnimationTimer = 0f;
            mixer.previousIdleAnimation = mixer.currentIdleAnimation;
            mixer.currentIdleAnimation = newIdleAnimation;
            psc.NewAnimationLoaded(newIdleAnimation, Live2DAnimationType.IdleAnimation, null);
        }

        public static void StartNormalAnimation(Live2DAnimationMixer mixer, ModelParamStateController psc, Live2DAnimation newAnimation, bool newAnimationStopsOnLastFrame, float fadeOutAfterHotkey)
        {
            if (mixer.animationFadeOngoing)
            {
                return;
            }
            if (newAnimation.Equals(mixer.currentAnimation) && mixer.animationStopsOnLastFrame && newAnimationStopsOnLastFrame)
            {
                StopAnimation(mixer, psc, Live2DAnimationType.OneShotAnimation);
                mixer.hasSetHoldAnimation = false;
                return;
            }
            mixer.previousAnimationCurves.Clear();
            mixer.previousAnimationCurves = new Dictionary<string, AnimationCurve>(mixer.animationCurves);
            mixer.animationCurves.Clear();
            for (int i = 0; i < newAnimation.data.ParameterIds.Length; i++)
            {
                AnimationCurve value = newAnimation.data.ParameterCurves[i];
                string text = newAnimation.data.ParameterIds[i];
                if (!mixer.animationCurves.ContainsKey(text))
                {
                    mixer.animationCurves.Add(text, value);
                }
                else
                {
                    Debug.LogError($"动画 {newAnimation.name} 有重复的参数:{text}.动画仍将播放，但此参数将被忽略。请在Live2D立体派编辑器中检查Live2D动画文件，如有必要，请进行修复");
                }
            }
            mixer.previousAnimationProgress = mixer.animationTimer;
            mixer.animationTimer = 0f;
            mixer.previousAnimation = mixer.currentAnimation;
            mixer.currentAnimation = newAnimation;
            mixer.previousAnimationLength = mixer.animationLength;
            if (fadeOutAfterHotkey > 0f)
            {
                mixer.animationLength = Mathf.Min(fadeOutAfterHotkey, newAnimation.clip.length);
            }
            else
            {
                mixer.animationLength = newAnimation.clip.length;
            }
            float minOrMax = mixer.animationLength / 2f - 0.01f;
            float minOrMax2 = 0.001f;
            mixer.animationFadeTime = newAnimation.fadeSpeed.ClampBetween(minOrMax2, minOrMax);
            mixer.animationStopsOnLastFrame = newAnimationStopsOnLastFrame;
            mixer.hasSetHoldAnimation = false;
            psc.NewAnimationLoaded(newAnimation, Live2DAnimationType.OneShotAnimation, mixer.previousAnimation);
        }

        public static void StepAnimationCurves(Live2DAnimationMixer mixer, ModelParamStateController psc, Dictionary<CubismParameter, float> currentParameterValues)
        {
            mixer.currentParameterValues = currentParameterValues;
            StepIdleCurves(mixer, psc);
            StepNormalCurves(mixer, psc);
            mixer.StepPartOpacityCurves(Live2DAnimationType.IdleAnimation);
            mixer.StepPartOpacityCurves(Live2DAnimationType.OneShotAnimation);
        }

        public static void StepIdleCurves(Live2DAnimationMixer mixer, ModelParamStateController psc)
        {
            mixer.idleAnimationTimer += Time.deltaTime;
            foreach (CubismParameter cubismParameter in mixer.vtsModel.Live2DModel.Parameters)
            {
                float value = mixer.vtsModel.PhysicsOutputParameters.Contains(cubismParameter) ? mixer.currentParameterValues[cubismParameter] : cubismParameter.DefaultValue;
                mixer.vtsModel.ParamStateController.ParamStateDictionary[cubismParameter].Targets[ModelParamStateController.IntPrio.prio_0_default] = value;
                AnimationCurve animationCurve;
                bool flag = mixer.idleAnimationCurves.TryGetValue(cubismParameter.Id, out animationCurve);
                float num = 0f;
                if (flag && mixer.currentIdleAnimation != null)
                {
                    float time = mixer.idleAnimationTimer % mixer.currentIdleAnimation.clip.length;
                    num = animationCurve.Evaluate(time);
                    bool flag2 = mixer.idleAnimationTimer <= 0.6f;
                    bool flag3 = mixer.previousIdleAnimation != null;
                    if (flag2 && flag3)
                    {
                        AnimationCurve animationCurve2;
                        bool flag4 = mixer.previousIdleAnimationCurves.TryGetValue(cubismParameter.Id, out animationCurve2);
                        float time2 = (mixer.idleAnimationTimer + mixer.previousIdleAnimationProgress) % mixer.previousIdleAnimation.clip.length;
                        if (flag4)
                        {
                            float num2 = animationCurve2.Evaluate(time2);
                            float num3 = mixer.idleAnimationTimer / 0.6f;
                            num = num * num3 + (1f - num3) * num2;
                        }
                    }
                    psc.ParamStateDictionary[cubismParameter].Targets[ModelParamStateController.IntPrio.prio_1_idle] = (flag ? num : mixer.currentParameterValues[cubismParameter]);
                }
            }
        }

        public static void StepNormalCurves(Live2DAnimationMixer mixer, ModelParamStateController psc)
        {
            if (mixer.currentAnimation == null)
            {
                return;
            }
            mixer.animationTimer += Time.deltaTime;
            float num = mixer.animationLength - mixer.currentAnimation.fadeSpeed;
            if (mixer.animationTimer >= num)
            {
                if (mixer.animationStopsOnLastFrame)
                {
                    if (!mixer.hasSetHoldAnimation)
                    {
                        psc.HoldAnimation(mixer.currentAnimation);
                        mixer.hasSetHoldAnimation = true;
                    }
                }
                else
                {
                    psc.NewAnimationLoaded(null, Live2DAnimationType.OneShotAnimation, null);
                }
            }
            bool flag = mixer.animationTimer >= mixer.animationLength;
            if (flag)
            {
                mixer.animationTimer = mixer.animationLength;
            }
            foreach (CubismParameter cubismParameter in mixer.vtsModel.Live2DModel.Parameters)
            {
                AnimationCurve animationCurve;
                bool hasCurve = mixer.animationCurves.TryGetValue(cubismParameter.Id, out animationCurve);
                float num2 = 0f;
                if (hasCurve && mixer.currentAnimation != null)
                {
                    float time = flag ? mixer.animationLength : (mixer.animationTimer % mixer.animationLength);
                    num2 = animationCurve.Evaluate(time);
                    mixer.animationFadeOngoing = (mixer.animationTimer <= mixer.animationFadeTime);
                    bool flag3 = mixer.previousAnimation != null;
                    AnimationCurve animationCurve2;
                    if (mixer.animationFadeOngoing && flag3 && mixer.previousAnimationCurves.TryGetValue(cubismParameter.Id, out animationCurve2))
                    {
                        float value = mixer.animationTimer + mixer.previousAnimationProgress;
                        float num3 = animationCurve2.Evaluate(value.ClampBetween(0f, mixer.previousAnimationLength));
                        float num4 = mixer.animationTimer / mixer.animationFadeTime;
                        num2 = num2 * num4 + (1f - num4) * num3;
                    }
                    psc.ParamStateDictionary[cubismParameter].Targets[ModelParamStateController.IntPrio.prio_4_animation] = (hasCurve ? num2 : mixer.currentParameterValues[cubismParameter]);
                }
            }
            if (flag && !mixer.animationStopsOnLastFrame)
            {
                StopAnimation(mixer, psc, Live2DAnimationType.OneShotAnimation);
            }
        }

        public static void StopAnimation(Live2DAnimationMixer mixer, ModelParamStateController psc, Live2DAnimationType type)
        {
            if (type == Live2DAnimationType.IdleAnimation)
            {
                mixer.currentIdleAnimation = null;
            }
            else if (type == Live2DAnimationType.OneShotAnimation)
            {
                mixer.currentAnimation = null;
                mixer.previousAnimation = null;
            }
            psc.NewAnimationLoaded(null, type, null);
        }
    }
}