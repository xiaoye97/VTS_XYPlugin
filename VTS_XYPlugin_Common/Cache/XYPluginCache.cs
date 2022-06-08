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
        public List<BDanMuMessage> DanMuMessages = new List<BDanMuMessage>();
        public List<BGiftMessage> GiftMessages = new List<BGiftMessage>();
        public List<BBuyJianDuiMessage> BuyJianDuiMessages = new List<BBuyJianDuiMessage>();
        public List<BRenQiMessage> RenQiMessages = new List<BRenQiMessage>();
        public List<BSCMessage> SCMessages = new List<BSCMessage>();
        public List<BWarningMessage> WarningMessages = new List<BWarningMessage>();
        public List<BWatchPeopleMessage> WatchPeopleMessages = new List<BWatchPeopleMessage>();
    }
}