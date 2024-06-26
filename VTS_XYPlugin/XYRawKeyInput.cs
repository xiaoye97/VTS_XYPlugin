﻿using System;
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