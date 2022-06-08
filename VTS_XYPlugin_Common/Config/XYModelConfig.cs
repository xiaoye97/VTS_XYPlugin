using System.Collections.Generic;

namespace VTS_XYPlugin_Common
{
    /// <summary>
    /// 记录在每个模型下面的配置
    /// </summary>
    public class XYModelConfig
    {
        public List<GiftTriggerActionData> TriggerActionData = new List<GiftTriggerActionData>();
        public DropItemDataBase OverrideDropItemDataBase = new DropItemDataBase();
    }
}