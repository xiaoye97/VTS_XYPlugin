using BepInEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using VTS_XYPlugin_Common;

namespace VTS_XYPlugin
{
    public class Bilibili : MonoSingleton<Bilibili>
    {
        public static int NowRoomID;
        public static string[] SplitStr = new[] { "$#**#$" };

        // 弹幕姬程序路径
        private static string ExePath;

        public static string dataCache;
        public static Queue<string> dataQueue = new Queue<string>();
        public Process process;

        // 如果进程意外退出的重连CD
        private float reConnectCD;

        // 如果60秒没接收到服务器消息的重连CD
        private static float reConnectCD2 = 120;

        // 是否允许连接B站，当启动参数里含有nobili的时候，不连接B站
        public static bool CanConnectBili = true;

        public override void Init()
        {
            dataCache = "";
            ExePath = $"{Paths.PluginPath}/VTS_XYPlugin/BLiveDMConsole/BLiveDMConsole.exe";
            XYLog.LogMessage($"弹幕姬路径:{ExePath}");
            if (XYPlugin.CmdArgs.Contains("-nobili"))
            {
                CanConnectBili = false;
                XYLog.LogMessage($"当前已禁用连接Bilibili");
            }
        }

        public void Update()
        {
            if (CanConnectBili)
            {
                reConnectCD -= Time.deltaTime;
                reConnectCD2 -= Time.deltaTime;
                if (reConnectCD < 0)
                {
                    // 每5秒检查一次
                    reConnectCD = 5f;
                    if (process == null || process.HasExited)
                    {
                        StartDM();
                    }
                }
                if (reConnectCD2 < 0)
                {
                    reConnectCD2 = 120;
                    XYLog.LogWarning($"120秒没有接收到服务器消息，重连服务器");
                    EndDM();
                    StartDM();
                }
            }
            while (dataQueue.Count > 0)
            {
                string data = dataQueue.Dequeue();
                string[] args = null;
                XYMessage message = null;
                switch (data[0])
                {
                    case 'D':
                        args = data.Split(SplitStr, 8, StringSplitOptions.None);
                        message = new BDanMuMessage(args);
                        break;

                    case 'G':
                        args = data.Split(SplitStr, 8, StringSplitOptions.None);
                        message = new BGiftMessage(args);
                        // 如果是礼物，则直接将头像传入cache
                        BilibiliHeadCache.Instance.OnRecvGift(message as BGiftMessage);
                        break;

                    case 'P':
                        args = data.Split(SplitStr, 2, StringSplitOptions.None);
                        message = new BWatchPeopleMessage(args);
                        break;

                    case 'R':
                        args = data.Split(SplitStr, 2, StringSplitOptions.None);
                        message = new BRenQiMessage(args);
                        break;

                    case 'J':
                        args = data.Split(SplitStr, 6, StringSplitOptions.None);
                        message = new BBuyJianDuiMessage(args);
                        break;

                    case 'S':
                        args = data.Split(SplitStr, 6, StringSplitOptions.None);
                        message = new BSCMessage(args);
                        break;

                    case 'W':
                        args = data.Split(SplitStr, 2, StringSplitOptions.None);
                        message = new BWarningMessage(args);
                        break;
                }
                //XYLog.LogMessage($"[Bili]{data}");
                if (message != null)
                {
                    MessageCenter.Instance.Send(message);
                }
                else
                {
                    XYLog.LogMessage($"[Bili]{data}");
                }
            }
        }

        public void StartDM()
        {
            EndDM();
            XYLog.LogMessage($"开始连接直播间{XYPlugin.Instance.GlobalConfig.BLiveConfig.RoomID}");
            NowRoomID = XYPlugin.Instance.GlobalConfig.BLiveConfig.RoomID;
            ThreadStart childRef = new ThreadStart(DMExeThread);
            Thread childThread = new Thread(childRef);
            childThread.Start();
        }

        public void EndDM()
        {
            // 如果进程不为空，则先杀死老进程，再开始新进程
            if (process != null)
            {
                if (!process.HasExited)
                {
                    process.Kill();
                    process.Close();
                    XYLog.LogMessage("断开当前直播间");
                }
                process = null;
            }
        }

        public void DMExeThread()
        {
            RunPythonScript(XYPlugin.Instance.GlobalConfig.BLiveConfig.RoomID.ToString());
        }

        public void RunPythonScript(string roomID)
        {
            process = new Process();
            process.StartInfo.FileName = ExePath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.Arguments = roomID;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Exited += Process_Exited;
            if (process.Start())
            {
                process.BeginOutputReadLine();
                process.OutputDataReceived += new DataReceivedEventHandler(Out_RecvData);
                Console.ReadLine();
                process.WaitForExit();
            }
            else
            {
                EndDM();
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            XYLog.LogMessage($"弹幕进程已退出");
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Out_RecvData(object sender, DataReceivedEventArgs e)
        {
            dataCache = e.Data;
            if (!string.IsNullOrWhiteSpace(dataCache))
            {
                lock (dataQueue)
                {
                    dataQueue.Enqueue(dataCache);
                    reConnectCD2 = 60f;
                }
            }
        }

        private void OnApplicationQuit()
        {
            EndDM();
        }
    }
}