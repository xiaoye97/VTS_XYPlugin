using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VTS_XYPlugin_Common;
using System.Collections.Generic;
using UnityRawInput;

namespace VTS_XYPlugin
{
    public class XYHotkeyManager : MonoSingleton<XYHotkeyManager>
    {
        public static bool EnableTriggerHotkey = true;
        // 触发快捷键的CD，在CD冷却之前，不会重复触发
        private List<float> hotkeyCDs = new List<float>();
        // 动作的CD，对于播放动画的快捷键，使用动画的时长
        private float actionCD;
        public List<WaitTriggerHotkeyData> WaitList = new List<WaitTriggerHotkeyData>();
        public HotkeyManager HotkeyManager;

        public override void Init()
        {
        }

        void Update()
        {
            if (HotkeyManager == null)
            {
                HotkeyManager = GameObject.FindObjectOfType<HotkeyManager>();
            }
            float dt = Time.deltaTime;
            actionCD -= dt;
            for (int i = 0; i < hotkeyCDs.Count; i++)
            {
                hotkeyCDs[i] -= dt;
            }
        }

        public void UpdateTrigger()
        {
            if (actionCD < 0)
            {
                if (WaitList.Count > 0)
                {
                    var wait = WaitList[0];
                    WaitList.RemoveAt(0);
                    XYLog.LogMessage($"轮到触发:{wait.data.ActionName}");
                    // 找到快捷键并执行
                    if (HotkeyManager != null)
                    {
                        foreach (var hotkey in HotkeyManager.hotkeys)
                        {
                            XYLog.LogMessage(hotkey.Name);
                            if (hotkey.Name == wait.data.ActionName)
                            {
                                XYLog.LogMessage($"触发快捷键:{hotkey.File}");
                                HotkeyManager.ExecuteHotkey(hotkey, null, XYModelManager.Instance.NowModel);
                                // 如果当前快捷键的动作为播放动画，则需要根据动画来设置CD
                                if (hotkey.Action == "TriggerAnimation")
                                {
                                    actionCD = XYModelManager.Instance.NowModel.AnimationMixer.animationLength;
                                    //XYLog.LogMessage($"动画时长为{actionCD}秒");
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void GetNowModelHotkeys()
        {
            if (XYModelManager.Instance.NowModelDef != null)
            {
                foreach (var key in XYModelManager.Instance.NowModelDef.Hotkeys)
                {
                    XYLog.LogMessage($"{key.Name} {key.File}");
                }
            }
        }

        public void ClearAllCD()
        {
            hotkeyCDs = new List<float>();
            for (int i = 0; i < XYModelManager.Instance.NowModelConfig.TriggerActionData.Count; i++)
            {
                hotkeyCDs.Add(0);
            }
        }

        /// <summary>
        /// 检查是否已经冷却
        /// </summary>
        /// <param name="id">绑定信息ID</param>
        /// <returns></returns>
        public bool IsCDFinished(int id)
        {
            if (hotkeyCDs.Count > id)
            {
                return hotkeyCDs[id] <= 0;
            }
            return false;
        }

        /// <summary>
        /// 触发快捷键(加入队列按顺序来)
        /// </summary>
        public void TriggerHotkey(GiftTriggerActionData data, Action<GiftTriggerActionData> onTriggerCallback = null)
        {
            if (!EnableTriggerHotkey) return;
            // 检查是否在冷却，如果在冷却，则直接忽略
            if (!IsCDFinished(data.ID))
            {
                return;
            }
            WaitTriggerHotkeyData wait = new WaitTriggerHotkeyData();
            wait.data = data;
            wait.onTriggerCallback = onTriggerCallback;
            WaitList.Add(wait);
        }

        /// <summary>
        /// 立即触发快捷键
        /// 使用名字的方式
        /// </summary>
        public void TriggerHotkeyByNameNow(string hotkeyName, Action<GiftTriggerActionData> onTriggerCallback = null)
        {
            if (!EnableTriggerHotkey) return;
            if (HotkeyManager == null) return;
            foreach (var hotkey in HotkeyManager.hotkeys)
            {
                if (hotkey.Name == hotkeyName)
                {
                    HotkeyManager.ExecuteHotkey(hotkey, null, XYModelManager.Instance.NowModel);
                    break;
                }
            }
        }

        /// <summary>
        /// 触发快捷键(插队)
        /// </summary>
        public void TriggerHotkeyNow(GiftTriggerActionData data, Action<GiftTriggerActionData> onTriggerCallback = null)
        {
            if (!EnableTriggerHotkey) return;
            // 检查是否在冷却，如果在冷却，则直接忽略
            if (!IsCDFinished(data.ID))
            {
                return;
            }
            WaitTriggerHotkeyData wait = new WaitTriggerHotkeyData();
            wait.data = data;
            wait.onTriggerCallback = onTriggerCallback;
            WaitList.Insert(0, wait);
        }
    }
}
