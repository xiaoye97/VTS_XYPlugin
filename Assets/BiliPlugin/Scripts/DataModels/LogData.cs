using System;

public class LogData
{
    public DateTime Time;
    public string Msg;
    public UnityEngine.LogType LogType;

    public LogData()
    {
    }

    public LogData(string msg, UnityEngine.LogType logType)
    {
        Time = DateTime.Now;
        Msg = msg;
        LogType = logType;
    }
}
