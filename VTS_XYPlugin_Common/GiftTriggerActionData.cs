using System;
using System.Collections.Generic;

namespace VTS_XYPlugin_Common
{
    public class GiftTriggerActionData
    {
        /// <summary>
        /// 用于计算CD的ID，在读取配置时自动分配
        /// </summary>
        [NonSerialized]
        public int ID;
        public GiftTriggerActionType GiftTriggerActionType = GiftTriggerActionType.收到特定礼物时触发;
        public float TriggerCD;
        public string ActionName = "";

        // 收到特定礼物时触发
        public string TriggerGiftName = "";

        // 收到礼物满足条件时触发
        public int MinMoneyLimit = 1;
        public int MaxMoneyLimit = 10;

        // 收到舰长时触发
        public BJianDuiType JianZhangType = BJianDuiType.舰长;

        public bool Enable = true;
    }

    public enum GiftTriggerActionType
    {
        收到特定礼物时触发,
        收到礼物满足条件时触发,
        收到SC时触发,
        收到舰长时触发
    }
}
