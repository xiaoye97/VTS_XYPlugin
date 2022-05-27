using System;
using Lean.Pool;
using UnityEngine;
using VTS_XYPlugin;
using System.Collections.Generic;

namespace VTS_ParameterTriggerAudio
{
    public class AudioPlayer
    {
        /// <summary>
        /// 是否可用，如果初始化的时候遇到错误，则不可用
        /// </summary>
        public bool CanUse;
        public ParameterTriggerConfig Config;
        public List<Func<bool>> CheckList = new List<Func<bool>>();

        public AudioPlayer(ParameterTriggerConfig config)
        {
            Config = config;
        }

        public void Init()
        {
            CanUse = false;
            // 验证参数是否匹配
            bool countOK = Config.Parameters.Count == Config.Values.Count && Config.Parameters.Count == Config.Operations.Count && Config.Parameters.Count == Config.IsInputParam.Count;
            if (countOK)
            {
                ParameterTriggerAudioBehaviour.Inst.LoadAudio(Config.AudioFile);
                // 解析参数
                for (int i = 0; i < Config.Parameters.Count; i++)
                {
                    string paramName = Config.Parameters[i];
                    string op = Config.Operations[i];
                    float value = Config.Values[i];
                    bool isInputParam = Config.IsInputParam[i];
                    FaceTrackingParamInfo.FaceTrackingParam faceParam = FaceTrackingParamInfo.FaceTrackingParam.UnknownParameter;
                    bool parseSucc = false;
                    if (isInputParam)
                    {
                        parseSucc = Enum.TryParse<FaceTrackingParamInfo.FaceTrackingParam>(paramName, out faceParam);
                        if (!parseSucc)
                        {
                            XYLog.LogError($"解析输入参数{paramName}失败，请注意拼写是否正确");
                        }
                    }
                    Func<bool> checkFunc = () =>
                    {
                        if (isInputParam && parseSucc && XYModelManager.Instance.NowModel.ModelAnimator.faceParameters != null)
                        {
                            if (XYModelManager.Instance.NowModel.ModelAnimator.faceParameters.faceParameters.ContainsKey(faceParam))
                            {
                                var param = XYModelManager.Instance.NowModel.ModelAnimator.faceParameters.faceParameters[faceParam];
                                switch (op)
                                {
                                    case "=": return param == value;
                                    case ">": return param > value;
                                    case "<": return param < value;
                                    case ">=": return param >= value;
                                    case "<=": return param <= value;
                                }
                                return false;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (XYModelManager.Instance.NowModel.NamedParams.ContainsKey(paramName))
                            {
                                var param = XYModelManager.Instance.NowModel.NamedParams[paramName];
                                switch (op)
                                {
                                    case "=": return param.Value == value;
                                    case ">": return param.Value > value;
                                    case "<": return param.Value < value;
                                    case ">=": return param.Value >= value;
                                    case "<=": return param.Value <= value;
                                }
                                return false;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    };
                    CheckList.Add(checkFunc);
                }
                CanUse = true;
            }
            else
            {
                XYLog.LogError($"ParameterTriggerAudio:音效:{Config.AudioFile}的参数数量不一致，将跳过此音效");
            }
        }

        public void CheckParameter()
        {
            // 检查当前帧的值是否在区间内
            bool nowValueTrue = true;
            foreach (var func in CheckList)
            {
                if (!func())
                {
                    nowValueTrue = false;
                    break;
                }
            }
            // 循环和非循环使用两种控制方案
            if (Config.Loop)
            {
                CheckLoopAudio(nowValueTrue);
            }
            else
            {
                CheckOneShotAudio(nowValueTrue);
            }
        }

        public AudioSourceAutoDestroy LoopAudioSource;
        public float loopFadeTimer;
        public LoopAudioState loopState = LoopAudioState.NoInParam;
        private void CheckLoopAudio(bool nowValueTrue)
        {
            if (LoopAudioSource == null)
            {
                var go = LeanPool.Spawn(ParameterTriggerAudio.AudioSourcePrefab);
                LoopAudioSource = go.GetComponent<AudioSourceAutoDestroy>();
                LoopAudioSource.Player = this;
                LoopAudioSource.audioSource.loop = true;
                LoopAudioSource.audioSource.clip = null;
                LoopAudioSource.audioSource.volume = Config.Volume;
                LoopAudioSource.autoDestroy = false;
            }
            switch (loopState)
            {
                case LoopAudioState.NoInParam:
                    // 如果之前不在范围，现在进入范围，则开始过渡到InParam
                    if (nowValueTrue)
                    {
                        LoopAudioSource.audioSource.volume = 0;
                        LoopAudioSource.audioSource.clip = ParameterTriggerAudioBehaviour.Audios[Config.AudioFile];
                        LoopAudioSource.audioSource.Play();
                        if (Config.LoopFadeTime <= 0)
                        {
                            loopFadeTimer = Config.Volume;
                            loopState = LoopAudioState.InParam;
                            //XYLog.LogMessage("1:NoInParam->InParam");
                        }
                        else
                        {
                            loopFadeTimer = 0;
                            loopState = LoopAudioState.FadeToOpen;
                            //XYLog.LogMessage("2:NoInParam->FadeToOpen");
                        }
                    }
                    break;
                case LoopAudioState.InParam:
                    // 如果之前在范围内但是现在不在，则过渡到NoInParam
                    if (!nowValueTrue)
                    {
                        if (Config.LoopFadeTime <= 0)
                        {
                            LoopAudioSource.audioSource.Stop();
                            loopState = LoopAudioState.NoInParam;
                            //XYLog.LogMessage("3:InParam->NoInParam");
                        }
                        else
                        {
                            loopFadeTimer = Config.LoopFadeTime;
                            loopState = LoopAudioState.FadeToClose;
                            //XYLog.LogMessage("4:InParam->FadeToClose");
                        }
                    }
                    break;
                case LoopAudioState.FadeToOpen:
                    loopFadeTimer += Time.deltaTime;
                    float openValue = Mathf.Clamp(Config.Volume * (loopFadeTimer / Config.LoopFadeTime), 0, 1);
                    //XYLog.LogMessage($"FadeToOpen: timer:{loopFadeTimer} volume:{openValue}");
                    LoopAudioSource.audioSource.volume = openValue;
                    if (loopFadeTimer >= Config.LoopFadeTime)
                    {
                        loopState = LoopAudioState.InParam;
                        //XYLog.LogMessage("5:FadeToOpen->InParam");
                    }
                    break;
                case LoopAudioState.FadeToClose:
                    loopFadeTimer -= Time.deltaTime;
                    float closeValue = Mathf.Clamp(Config.Volume * (loopFadeTimer / Config.LoopFadeTime), 0, 1);
                    //XYLog.LogMessage($"FadeToClose: timer:{loopFadeTimer} volume:{closeValue}");
                    LoopAudioSource.audioSource.volume = closeValue;
                    if (loopFadeTimer <= 0)
                    {
                        loopState = LoopAudioState.NoInParam;
                        //XYLog.LogMessage("6:FadeToClose->NoInParam");
                    }
                    break;
            }
        }

        public int NowOneShotAudioCount;
        OneShotAudioState oneShotState;
        private void CheckOneShotAudio(bool nowValueTrue)
        {
            switch (oneShotState)
            {
                case OneShotAudioState.NoInParam:
                    if (nowValueTrue)
                    {
                        if (Config.Muti || NowOneShotAudioCount <= 0)
                        {
                            var go = LeanPool.Spawn(ParameterTriggerAudio.AudioSourcePrefab);
                            LoopAudioSource = go.GetComponent<AudioSourceAutoDestroy>();
                            LoopAudioSource.Player = this;
                            LoopAudioSource.audioSource.loop = false;
                            LoopAudioSource.autoDestroy = true;
                            LoopAudioSource.audioSource.clip = ParameterTriggerAudioBehaviour.Audios[Config.AudioFile];
                            LoopAudioSource.audioSource.volume = Config.Volume;
                            LoopAudioSource.audioSource.Play();
                            oneShotState = OneShotAudioState.InParamAndPlayed;
                            NowOneShotAudioCount++;
                        }
                    }
                    break;
                case OneShotAudioState.InParamAndPlayed:
                    if (!nowValueTrue)
                    {
                        oneShotState = OneShotAudioState.NoInParam;
                    }
                    break;
            }
        }
    }

    public enum LoopAudioState
    {
        NoInParam,
        InParam,
        FadeToOpen,
        FadeToClose
    }

    public enum OneShotAudioState
    {
        NoInParam,
        InParamAndPlayed,
    }
}
