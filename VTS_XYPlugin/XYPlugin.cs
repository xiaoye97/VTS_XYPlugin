using BepInEx;
using HarmonyLib;
using PENet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;
using VTS_XYPlugin_Common;

namespace VTS_XYPlugin
{
    [BepInPlugin(GUID, PluginName, VERSION)]
    [ExScript(PluginName, "宵夜97", "基础插件", VERSION)]
    public class XYPlugin : BaseUnityPlugin
    {
        public const string GUID = "me.xiaoye97.plugin.VTubeStudio.VTS_XYPlugin";
        public const string PluginName = "VTS_XYPlugin";
        public const string VERSION = "2.0.0";

        public static List<string> CmdArgs;

        public static XYPlugin Instance;

        /// <summary>
        /// 全局配置文件监听
        /// </summary>
        public XYFileWatcher GlobalConfigWatcher;

        /// <summary>
        /// 掉落物配置文件监听
        /// </summary>
        public XYFileWatcher DropItemDataBaseWatcher;

        /// <summary>
        /// 全局配置
        /// </summary>
        public XYGlobalConfig GlobalConfig;

        /// <summary>
        /// 掉落物配置
        /// </summary>
        public DropItemDataBase DropItemDataBase;

        public PESocket<XYServerSession, NetMsg> ServerSkt = null;

        // 和GUI交互用的端口
        public static int GUINetPort;

        public Process GUIProcess;

        // VTS的左侧圆形按钮控制器
        private CircleButtonController CircleButtonController;

        private RectTransform CircleButtonControllerRT;

        private void Awake()
        {
            Instance = this;
            CmdArgs = Environment.GetCommandLineArgs().ToList();
            XYLog.Init(Logger);
            DeleteErrorExScript();
            LogStartupMessage();
            Harmony.CreateAndPatchAll(typeof(XYPatch));
        }

        /// <summary>
        /// 删除BepInEx/plugins下的扩展脚本，它们应该放在BepInEx/plugins/VTS_Ex文件夹
        /// </summary>
        public void DeleteErrorExScript()
        {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Paths.PluginPath);
            var files = dir.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    if (files[i].Name.StartsWith("VTS_"))
                    {
                        files[i].Delete();
                    }
                }
                catch (Exception ex)
                {
                    XYLog.LogError(ex.ToString());
                }
            }
        }

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            XYLog.Update();
            if (GlobalConfigWatcher != null) GlobalConfigWatcher.Update(Time.deltaTime);
            if (DropItemDataBaseWatcher != null) DropItemDataBaseWatcher.Update(Time.deltaTime);
            if (CircleButtonControllerRT == null)
            {
                CircleButtonController = GameObject.FindObjectOfType<CircleButtonController>();
                if (CircleButtonController != null)
                {
                    CircleButtonControllerRT = CircleButtonController.transform as RectTransform;
                }
            }
        }

        private void OnGUI()
        {
            if (CircleButtonControllerRT != null && CircleButtonControllerRT.anchoredPosition.x >= 0)
            {
                if (GUIProcess == null)
                {
                    if (ServerSkt.session == null)
                    {
                        if (GUILayout.Button("打开XYPluginGUI"))
                        {
                            OpenGUI();
                        }
                    }
                }
            }
        }

        private void OnApplicationQuit()
        {
            if (GUIProcess != null)
            {
                if (!GUIProcess.HasExited)
                {
                    GUIProcess.Kill();
                    GUIProcess.Close();
                }
            }
        }

        private void Init()
        {
            XYExScriptManager.Instance.Init();
            XYCache.Instance.Init();
            XYModelManager.Instance.Init();
            XYDropManager.Instance.Init();
            XYVideoManager.Instance.Init();
            XYHotkeyManager.Instance.Init();
            BilibiliHeadCache.Instance.Init();

            ServerSkt = new PESocket<XYServerSession, NetMsg>();
            ServerSkt.SetLog(true, (string msg, int lv) =>
            {
                switch (lv)
                {
                    case 0:
                        msg = "[PENet]Log:" + msg;
                        XYLog.LogMessage(msg);
                        break;

                    case 1:
                        msg = "[PENet]Warn:" + msg;
                        XYLog.LogWarning(msg);
                        break;

                    case 2:
                        msg = "[PENet]Error:" + msg;
                        XYLog.LogError(msg);
                        break;

                    case 3:
                        msg = "[PENet]Info:" + msg;
                        XYLog.LogMessage(msg);
                        break;
                }
            });
            GUINetPort = NetHelper.GetAvailablePort("127.0.0.1");
            ServerSkt.StartAsServer("127.0.0.1", GUINetPort);
            XYLog.LogMessage($"在端口{GUINetPort}上启用XYPlugin服务器");

            // 监控全局配置文件
            GlobalConfigWatcher = new XYFileWatcher(XYPaths.GlobalConfigPath);
            GlobalConfigWatcher.OnFileModified += OnGlobalConfigFileModified;
            // 监控掉落物配置文件
            DropItemDataBaseWatcher = new XYFileWatcher(XYPaths.DropItemConfigPath);
            DropItemDataBaseWatcher.OnFileModified += OnDropItemConfigFileModified;
            FileHelper.LoadGlobalConfig();
            FileHelper.LoadDropItemConfig();
            // 初始化Bilibili管理器
            Bilibili.Instance.Init();
            if (Bilibili.CanConnectBili)
            {
                Bilibili.Instance.StartDM();
            }
            if (GlobalConfig.AutoOpenGUI)
            {
                Invoke("OpenGUI", 1);
            }
            XYRawKeyInput.Instance.Init();
        }

        private void LogStartupMessage()
        {
            string log = "\n========================================\n";
            log += $"XYPlugin已启动\n";
            log += $"XYPlugin版本:{VERSION}\n";
            log += $"XYPlugin日志路径:{XYLog.LogFilePath}\n";
            log += "========================================\n";
            log += $"启动参数:{Environment.CommandLine}\n";
            log += $"当前VTS日志路径:{Application.consoleLogPath}\n";
            log += $"Unity版本:{Application.unityVersion}\n";
            log += $"VTS版本:{Application.version}\n";
            log += "========================================\n";
            XYLog.LogMessage(log);
            if (Application.consoleLogPath.HasChinese())
            {
                XYLog.LogError($"检测到VTS日志路径中含有中文，会导致VTSAPI无法正常使用，请注意检查电脑用户名是否包含中文！");
            }
        }

        private void OnGlobalConfigFileModified()
        {
            //XYLog.LogMessage("检测到全局配置文件变更");
            FileHelper.LoadGlobalConfig();
            // 如果配置文件中的房间号和当前房间号不一致，则重启弹幕姬
            if (GlobalConfig.BLiveConfig.RoomID != Bilibili.NowRoomID)
            {
                if (Bilibili.CanConnectBili)
                {
                    XYLog.LogWarning($"配置文件中的房间号更改，切换直播间{Bilibili.NowRoomID}到{GlobalConfig.BLiveConfig.RoomID}");
                    Bilibili.Instance.EndDM();
                    Bilibili.Instance.StartDM();
                }
                else
                {
                    XYLog.LogWarning($"配置文件中的房间号更改，但此实例禁用了bilibili连接，跳过");
                }
            }
        }

        private void OnDropItemConfigFileModified()
        {
            //XYLog.LogMessage("检测到礼物掉落配置文件变更");
            FileHelper.LoadDropItemConfig();
        }

        /// <summary>
        /// 打开GUI界面
        /// </summary>
        public void OpenGUI()
        {
            ThreadStart childRef = new ThreadStart(GUIExeThread);
            Thread childThread = new Thread(childRef);
            childThread.Start();
        }

        public void GUIExeThread()
        {
            GUIProcess = new Process();
            GUIProcess.StartInfo.FileName = XYPaths.GUIExePath;
            GUIProcess.StartInfo.Arguments = $"-XYPluginGUIPort {GUINetPort}";
            if (GUIProcess.Start())
            {
                XYLog.LogMessage("GUI启动成功");
            }
            else
            {
                XYLog.LogError("GUI启动失败");
            }
        }
    }
}