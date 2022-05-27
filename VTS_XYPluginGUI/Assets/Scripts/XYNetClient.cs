// By 宵夜97
using PENet;
using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using VTS_XYPlugin_Common;
using System.Collections.Generic;

public class XYNetClient : MonoSingleton<XYNetClient>
{
    public static Queue<string> MsgQueue = new Queue<string>();
    PESocket<XYNetClientSession, NetMsg> skt = null;
    public int Port = 10811;
    public bool StartConnect;
    public bool StopConnect;

    private float waitConnectCD = 3f;
    public static bool Connected;

    private void Start()
    {
        // 根据命令行参数来查找端口
        var args = Environment.GetCommandLineArgs();
        bool findPort = false;
        if (args != null)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-XYPluginGUIPort")
                {
                    if (i + 1 < args.Length)
                    {
                        string portStr = args[i + 1];
                        int port = 0;
                        if (int.TryParse(portStr, out port))
                        {
                            if (port > 0 && port <= 65535)
                            {
                                Port = port;
                                findPort = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
        if (findPort)
        {
            StartSocket();
        }
        else
        {
#if UNITY_EDITOR
#else
            UIWindowMessageBox.Instance.ShowOk("<color=red>错误</color>", $"启动参数中不包含VTS端口信息，请不要手动打开GUI的exe，GUI应该由XYPlugin来启动。\n启动参数:\n{Environment.CommandLine}", () =>
            {
                XYPlugin.Instance.Quit();
            });
#endif
        }
    }

    void Update()
    {
        if (StartConnect)
        {
            StartConnect = false;
            StartSocket();
        }
        if (StopConnect)
        {
            StopConnect = false;
            StopSocket();
        }
        waitConnectCD -= Time.deltaTime;
        if (waitConnectCD < 0)
        {
            waitConnectCD = float.MaxValue;
            if (!Connected)
            {
                UIWindowMessageBox.Instance.ShowOk("<color=red>连接失败</color>", $"超过3秒未成功连接到XYPlugin插件，请重启电脑重试。\n启动参数:\n{Environment.CommandLine}");
            }
        }
    }

    private void OnDestroy()
    {
        StopSocket();
    }

    public void SendCache(XYGUICache cache)
    {
        if (skt != null && skt.session != null)
        {
            string json = JsonConvert.SerializeObject(cache);
            skt.session.SendMsg(new NetMsg() { text = json });
        }
        else
        {
            UINotification.Instance.Show("<color=red>当前没有连接到VTS，无法发送请求</color>");
        }
    }

    public void StartSocket()
    {
        if (skt != null)
        {
            StopSocket();
        }
        skt = new PESocket<XYNetClientSession, NetMsg>();
        skt.SetLog(true, (string msg, int lv) =>
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
        skt.StartAsClient("127.0.0.1", Port);
    }

    public void StopSocket()
    {
        if (skt != null)
        {
            skt.Close();
            skt = null;
        }
    }
}

public class XYNetClientSession : PESession<NetMsg>
{
    protected override void OnConnected()
    {
        XYNetClient.Connected = true;
    }

    protected override void OnReciveMsg(NetMsg msg)
    {
        PETool.LogMsg("接收到PENet服务器消息:" + msg.text);
        XYNetClient.MsgQueue.Enqueue(msg.text);
    }

    protected override void OnDisConnected()
    {
        XYNetClient.Connected = false;
    }
}