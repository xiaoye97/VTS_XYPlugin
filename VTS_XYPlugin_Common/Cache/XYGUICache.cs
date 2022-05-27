using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
