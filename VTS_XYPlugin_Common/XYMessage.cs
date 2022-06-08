using System;

namespace VTS_XYPlugin_Common
{
    [Serializable]
    public class XYMessage
    {
        public string 消息名;

        // 是否在控制台打印此消息的日志
        [NonSerialized]
        public bool LogMessage = true;

        public XYMessage()
        {
            消息名 = GetType().Name;
        }
    }
}