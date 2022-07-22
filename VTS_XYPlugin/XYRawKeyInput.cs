using System;
using System.Collections.Generic;
using UnityRawInput;
using VTS_XYPlugin_Common;
using UnityEngine;

namespace VTS_XYPlugin
{
    public class XYRawKeyInput : MonoSingleton<XYRawKeyInput>
    {
        private static Dictionary<RawKey, int> KeyIndexDict = new Dictionary<RawKey, int>();
        private static List<RawKey> KeyList = new List<RawKey>();
        private static List<bool> LastFramePressed = new List<bool>();
        private static List<bool> ThisFramePressed = new List<bool>();
        public Action CheckInputAction;
        public bool ShowDebugGUI;

        public override void Init()
        {
            var values = Enum.GetValues(typeof(RawKey));
            for (int i = 0; i < values.Length; i++)
            {
                RawKey key = (RawKey)values.GetValue(i);
                KeyList.Add(key);
                KeyIndexDict[key] = i;
                LastFramePressed.Add(false);
                ThisFramePressed.Add(false);
            }
        }

        public void Update()
        {
            for (int i = 0; i < KeyList.Count; i++)
            {
                RawKey key = KeyList[i];
                bool pressed = RawKeyInput.IsKeyPressed(key);
                ThisFramePressed[i] = pressed;
            }
            RefreshXYHotkey();
            if (CheckInputAction != null)
            {
                try
                {
                    CheckInputAction();
                }
                catch (Exception ex)
                {
                    XYLog.LogError($"检测按键时出现异常:{ex}");
                }
            }
        }

        public void LateUpdate()
        {
            for (int i = 0; i < KeyList.Count; i++)
            {
                LastFramePressed[i] = ThisFramePressed[i];
            }
        }

        public void OnGUI()
        {
            if (ShowDebugGUI)
            {
                GUILayout.Space(20);
                GUILayout.BeginVertical(GUI.skin.box);
                for (int i = 0; i < KeyList.Count; i++)
                {
                    if (ThisFramePressed[i])
                    {
                        GUILayout.Label(KeyList[i].ToString());
                    }
                }
                GUILayout.EndVertical();
            }
        }

        public void RefreshXYHotkey()
        {
            RawKey switchMessageKey = (RawKey)XYPlugin.Instance.GlobalConfig.SwitchMessageSystemHotkey;
            RawKey switchDropKey = (RawKey)XYPlugin.Instance.GlobalConfig.SwitchDropGiftHotkey;
            RawKey switchTriggerKey = (RawKey)XYPlugin.Instance.GlobalConfig.SwitchTriggerActionHotkey;
            // 按键检测控制是否开关消息系统
            if (XYRawKeyInput.GetKey(RawKey.Control) && XYRawKeyInput.GetKeyDown(switchMessageKey))
            {
                MessageCenter.RunMessageCenter = !MessageCenter.RunMessageCenter;
                if (MessageCenter.RunMessageCenter) XYLog.LogMessage("打开了消息系统");
                else XYLog.LogMessage("关闭了消息系统");
            }
            // 按键检测控制是否开关礼物掉落
            if (XYRawKeyInput.GetKey(RawKey.Control) && XYRawKeyInput.GetKeyDown(switchDropKey))
            {
                XYDropManager.EnableDrop = !XYDropManager.EnableDrop;
                if (XYDropManager.EnableDrop) XYLog.LogMessage("打开了礼物掉落");
                else XYLog.LogMessage("关闭了礼物掉落");
            }
            // 按键检测控制是否开关动作触发
            if (XYRawKeyInput.GetKey(RawKey.Control) && XYRawKeyInput.GetKeyDown(switchTriggerKey))
            {
                XYHotkeyManager.EnableTriggerHotkey = !XYHotkeyManager.EnableTriggerHotkey;
                if (XYHotkeyManager.EnableTriggerHotkey) XYLog.LogMessage("打开了动作触发");
                else XYLog.LogMessage("关闭了动作触发");
            }
        }

        public static bool GetKey(RawKey key)
        {
            return ThisFramePressed[KeyIndexDict[key]];
        }

        public static bool GetKeyDown(RawKey key)
        {
            int index = KeyIndexDict[key];
            return ThisFramePressed[index] && !LastFramePressed[index];
        }

        public static bool GetKeyUp(RawKey key)
        {
            int index = KeyIndexDict[key];
            return !ThisFramePressed[index] && LastFramePressed[index];
        }

        public static bool GetKey(RawKeyMap key)
        {
            RawKey k = (RawKey)key;
            return GetKey(k);
        }

        public static bool GetKeyDown(RawKeyMap key)
        {
            RawKey k = (RawKey)key;
            return GetKeyDown(k);
        }

        public static bool GetKeyUp(RawKeyMap key)
        {
            RawKey k = (RawKey)key;
            return GetKeyUp(k);
        }

        public static List<RawKey> StringListToRawKeyList(List<string> list)
        {
            List<RawKey> result = new List<RawKey>();
            // 解析快捷键
            for (int i = 0; i < list.Count; i++)
            {
                if (Enum.TryParse<RawKey>(list[i], out RawKey key))
                {
                    result.Add(key);
                }
                else
                {
                    XYLog.LogError($"转换按键时出错，{list[i]}不是一个正确的按键");
                }
            }
            return result;
        }

        public static List<RawKey> RawKeyMapListToRawKeyList(List<RawKeyMap> list)
        {
            List<RawKey> result = new List<RawKey>();
            // 解析快捷键
            for (int i = 0; i < list.Count; i++)
            {
                result.Add((RawKey)list[i]);
            }
            return result;
        }
    }
}