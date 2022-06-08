using System.Collections.Generic;

namespace VTS_XYPlugin_Common
{
    /// <summary>
    /// GUI向插件发送的数据缓存
    /// </summary>
    public class XYGUICache
    {
        public List<TestDropCache> TestDrops = new List<TestDropCache>();
        public List<string> TestTriggerHotkeys = new List<string>();
    }
}