using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;  //需要添加这个名词空间，调用DataReceivedEventArg 
using System.Text;
using UnityEngine.UI;

public class BiliPython
{
    public int RoomID = 362064;

    private string sArguments;
    private string AssetsPath = "";
    public static string dataCache;
    public static Queue<string> dataQueue = new Queue<string>();
    Process p;
    public Action<string> OnReceivedData;

    public BiliPython()
    {
        dataCache = "";
        AssetsPath = Application.streamingAssetsPath;
        RoomID = ES3.Load<int>("RoomID", 362064);
    }

    public void StartPy()
    {
        if (p != null)
        {
            p.Kill();
            p.Close();
            BiliPlugin.Log($"断开当前直播间");
        }
        BiliPlugin.Log($"开始连接直播间{RoomID}");
        ThreadStart childRef = new ThreadStart(PythonThread);
        Thread childThread = new Thread(childRef);
        childThread.Start();
    }

    public void EndPy()
    {
        if (p != null)
        {
            p.Kill();
            p.Close();
        }
        BiliPlugin.Log($"停止连接直播间{RoomID}");
    }

    public void PythonThread()
    {
        sArguments = RoomID.ToString();
        RunPythonScript(sArguments, "-u");
    }

    public void RunPythonScript(string sArgName, string args = "")
    {
        p = new Process();
        p.StartInfo.FileName = AssetsPath + @"\blivedm\blivedm.exe";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.Arguments = sArgName;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        p.BeginOutputReadLine();
        p.OutputDataReceived += new DataReceivedEventHandler(Out_RecvData);
        Console.ReadLine();
        p.WaitForExit();
    }

    void Out_RecvData(object sender, DataReceivedEventArgs e)
    {
        if (dataCache != e.Data)
        {
            dataCache = e.Data;
            if (!string.IsNullOrWhiteSpace(dataCache))
            {
                lock (dataQueue)
                {
                    dataQueue.Enqueue(dataCache);
                }
            }
        }
    }

    public void ChangeRoomID(string msg)
    {
        int.TryParse(msg, out RoomID);
        ES3.Save<int>("RoomID", RoomID);
        BiliPlugin.Log($"设置房间号：{RoomID}");
    }

    public void OnApplicationQuit()
    {
        if (p != null)
        {
            p.Kill();
            p.Close();
        }
    }
}
