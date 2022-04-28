using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;
using System.Diagnostics;

namespace VTS_XYPluginGameSide
{
    [BepInPlugin("com.xiaoye97.plugin.VTubeStudio.VTS_XYPluginGameSide", "VTS_XYPluginGameSide", "1.3")]
    public class XYPlugin : BaseUnityPlugin
    {
        public static XYPlugin Instance;
        public ManualLogSource LogSource;
        public DropItemManager DropItemManager;
        public Process UnitySide;

        void Start()
        {
            Instance = this;
            LogSource = Logger;
            Logger.LogMessage("VTS_XYPlugin软件端插件已加载");
            Harmony.CreateAndPatchAll(typeof(VTSPatch));
            DropItemManager = new DropItemManager();
            Application.quitting += Application_quitting;
        }

        private void Application_quitting()
        {
            if (UnitySide != null)
            {
                UnitySide.Kill();
                UnitySide.Close();
            }
        }

        void Update()
        {
            XYAPI.Update();
            DropItemManager.Update();
        }

        public void OpenPluginUnitySide()
        {
            try
            {
                // 如果根目录有XYDev.txt则跳过启动
                if (System.IO.File.Exists($"{Paths.GameRootPath}/XYDev.txt"))
                {
                    return;
                }
                var processes = Process.GetProcessesByName("VTS_XYPlugin");
                if (processes.Length == 0)
                {
                    UnitySide = Process.Start($"{Paths.GameRootPath}/VTS_XYPlugin/VTS_XYPlugin.exe");
                }
            }
            catch { }
        }

        public void Log(string msg)
        {
            Logger.LogMessage(msg);
        }
    }
}
