using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VTS_XYPlugin_Common;
using UnityEngine.Video;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;
using Live2D.Cubism.Rendering;

namespace VTS_XYPlugin
{
    //[BindModel("尚未团子")]
    public class XYDebugBehaviour : XYCustomBehaviour
    {
        private bool showWindow = false;
        private Rect windowRect = new Rect(50, 50, 500, 400);

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                showWindow = !showWindow;
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                GameObject go = new GameObject("Test");
                var c = go.AddComponent<NoneBehaviour>();
                c.StartCoroutine(PlayTestVideo2());
            }
        }

        public void OnGUI()
        {
            if (showWindow)
            {
                windowRect = GUILayout.Window(666, windowRect, WindowFunc, "测试窗口(P)");
            }
        }

        public void WindowFunc(int id)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("加载全局配置"))
            {
                FileHelper.LoadGlobalConfig();
            }
            if (GUILayout.Button("保存全局配置"))
            {
                FileHelper.SaveGlobalConfig();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("加载当前模型配置"))
            {
                FileHelper.LoadNowModelConfig();
            }
            if (GUILayout.Button("保存当前模型配置"))
            {
                FileHelper.SaveNowModelConfig();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("加载掉落物配置"))
            {
                FileHelper.LoadDropItemConfig();
            }
            if (GUILayout.Button("保存掉落物配置"))
            {
                FileHelper.SaveDropItemConfig();
            }
            if (GUILayout.Button("重载掉落物数据"))
            {
                XYDropManager.Instance.ReloadDropItems();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("测试送礼牛哇牛哇"))
            {
                BGiftMessage gift = new BGiftMessage();
                gift.用户ID = 1371894082;
                gift.用户名 = "bili_89093521589";
                gift.礼物名 = "牛哇牛哇";
                gift.礼物数量 = 100;
                gift.瓜子类型 = BGiftCoinType.金瓜子;
                gift.瓜子数量 = 100;
                gift.头像图片链接 = "http://i1.hdslb.com/bfs/face/3bc9866948d0ebeaf6a271014df30150643cf743.jpg";
                MessageCenter.Instance.Send(gift);
                //DropManager.Instance.StartDrop(XYPlugin.Instance.GlobalConfig.GiftDataBase.Gifts[0], 100);
            }
            if (GUILayout.Button("获取当前模型快捷键"))
            {
                XYHotkeyManager.Instance.GetNowModelHotkeys();
            }
            if (GUILayout.Button("创建测试快捷键绑定"))
            {
                GiftTriggerActionData trigger = new GiftTriggerActionData();
                trigger.ID = 0;
                trigger.TriggerGiftName = "牛哇牛哇";
                trigger.GiftTriggerActionType = GiftTriggerActionType.收到特定礼物时触发;
                trigger.ActionName = "举手";
                trigger.TriggerCD = 3f;
                XYModelManager.Instance.NowModelConfig.TriggerActionData.Add(trigger);
            }
            if (GUILayout.Button("测试视频播放"))
            {
                GameObject go = new GameObject("Test");
                var c = go.AddComponent<NoneBehaviour>();
                c.StartCoroutine(PlayTestVideo2());
                //StartCoroutine(PlayTestVideo2());
            }
            if (GUILayout.Button("测试GIF"))
            {
                Gif gif = new Gif();
                gif.LoadGif($"{XYPaths.DropItemImageDirPath}/动画1.gif");
            }
            XYPlugin.Instance.GlobalConfig.DebugMode = GUILayout.Toggle(XYPlugin.Instance.GlobalConfig.DebugMode, "显示隐藏挂件");
            GUI.DragWindow();
        }


        public void PlayerTestVideo()
        {
            XYVideoManager.Instance.PlayVideo("file://C:/Users/xiaoye/Desktop/VTS插件素材/鲸落最终版 动态水印.mp4", false, 0);
        }

        public IEnumerator PlayTestVideo2()
        {
            XYVideoManager.Instance.PlayVideo("file://C:/Users/xiaoye/Desktop/VTS插件素材/transition-otameshi.webm", false, 30000);
            yield return new WaitForSeconds(1);
            var v = XYVideoManager.Instance.PlayVideo("file://C:/Users/xiaoye/Desktop/VTS插件素材/鲸落最终版 动态水印.mp4", false, 0);
            foreach (var key in XYHotkeyManager.Instance.HotkeyManager.hotkeys)
            {
                XYLog.LogMessage($"1遍历到:{key.Name}");
                if (key.Name == "切换模型")
                {
                    XYHotkeyManager.Instance.HotkeyManager.ExecuteHotkey(key);
                }
            }
            yield return new WaitForSeconds(5f);
            foreach (var key in XYHotkeyManager.Instance.HotkeyManager.hotkeys)
            {
                XYLog.LogMessage($"2遍历到:{key.Name}");
                if (key.Name == "摇头")
                {
                    XYHotkeyManager.Instance.HotkeyManager.ExecuteHotkey(key);
                }
            }
            //XYHotkeyManager.Instance.HotkeyManager.ExecuteHotkey(XYHotkeyManager.Instance.HotkeyManager.hotkeys.Find((h) => h.Name == "摇头"));
            yield return new WaitForSeconds(14f);
            XYVideoManager.Instance.PlayVideo("file://C:/Users/xiaoye/Desktop/VTS插件素材/transition-otameshi.webm", false, 30000);
            yield return new WaitForSeconds(1);
            foreach (var key in XYHotkeyManager.Instance.HotkeyManager.hotkeys)
            {
                XYLog.LogMessage($"3遍历到:{key.Name}");
                if (key.Name == "切换模型")
                {
                    XYHotkeyManager.Instance.HotkeyManager.ExecuteHotkey(key);
                }
            }
            //XYHotkeyManager.Instance.HotkeyManager.ExecuteHotkey(XYHotkeyManager.Instance.HotkeyManager.hotkeys.Find((h) => h.Name == "切换模型"));
            yield break;
        }
    }
}
