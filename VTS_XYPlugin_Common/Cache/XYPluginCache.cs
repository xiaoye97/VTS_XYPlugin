using System.Collections.Generic;

namespace VTS_XYPlugin_Common
{
    /// <summary>
    /// 插件向GUI发送的数据缓存
    /// </summary>
    public class XYPluginCache
    {
        public bool HasData;
        public int BiliDanMuJiPID = 0;
        public string NowModelConfigFilePath = "";
        public List<ExScriptAttribute> InstallExScripts = new List<ExScriptAttribute>();
        public List<string> NowModelHotkeys = new List<string>();
        public List<string> Logs = new List<string>();
    }
}