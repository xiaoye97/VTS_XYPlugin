using VTS_XYPlugin_Common;
using Newtonsoft.Json;

namespace VTS_XYPlugin
{
    /// <summary>
    /// 礼物触发快捷键脚本
    /// </summary>
    public class XYTriggerHotkeyBehaviour : XYCustomBehaviour
    {
        private MessageListener BGiftMessageListener;
        private MessageListener BJianZhangMessageListener;
        private MessageListener BSCMessageListener;

        public virtual void OnEnable()
        {
            BGiftMessageListener = MessageCenter.Instance.Register<BGiftMessage>(当收到礼物);
            BJianZhangMessageListener = MessageCenter.Instance.Register<BBuyJianDuiMessage>(当收到舰长);
            BSCMessageListener = MessageCenter.Instance.Register<BSCMessage>(当收到SC);
        }

        public virtual void OnDisable()
        {
            BGiftMessageListener.UnRegister();
            BJianZhangMessageListener.UnRegister();
            BSCMessageListener.UnRegister();
        }

        public virtual void 当收到礼物(object obj)
        {
            BGiftMessage message = (BGiftMessage)obj;
            if (XYModelManager.Instance.NowModelConfig != null)
            {
                foreach (var data in XYModelManager.Instance.NowModelConfig.TriggerActionData)
                {
                    if (data.Enable)
                    {
                        if (data.GiftTriggerActionType == GiftTriggerActionType.收到特定礼物时触发)
                        {
                            if (message.礼物名 == data.TriggerGiftName)
                            {
                                //XYLog.LogMessage($"收到礼物 {message.礼物名} 尝试触发:{JsonConvert.SerializeObject(data)}");
                                XYHotkeyManager.Instance.TriggerHotkey(data);
                            }
                        }
                        else if (data.GiftTriggerActionType == GiftTriggerActionType.收到礼物满足条件时触发)
                        {
                            if (message.瓜子类型 == BGiftCoinType.金瓜子)
                            {
                                float rmb = message.瓜子数量 / 1000f;
                                if (rmb >= data.MinMoneyLimit && rmb <= data.MaxMoneyLimit)
                                {
                                    //XYLog.LogMessage($"收到RMB {rmb} 尝试触发:{JsonConvert.SerializeObject(data)}");
                                    XYHotkeyManager.Instance.TriggerHotkey(data);
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void 当收到舰长(object obj)
        {
            BBuyJianDuiMessage message = (BBuyJianDuiMessage)obj;
            foreach (var data in XYModelManager.Instance.NowModelConfig.TriggerActionData)
            {
                if (data.Enable)
                {
                    if (data.GiftTriggerActionType == GiftTriggerActionType.收到舰长时触发)
                    {
                        //XYLog.LogMessage($"收到舰长 尝试触发:{JsonConvert.SerializeObject(data)}");
                        XYHotkeyManager.Instance.TriggerHotkey(data);
                    }
                }
            }
        }

        public virtual void 当收到SC(object obj)
        {
            BSCMessage message = (BSCMessage)obj;
            foreach (var data in XYModelManager.Instance.NowModelConfig.TriggerActionData)
            {
                if (data.Enable)
                {
                    if (data.GiftTriggerActionType == GiftTriggerActionType.收到SC时触发)
                    {
                        //XYLog.LogMessage($"收到SC 尝试触发:{JsonConvert.SerializeObject(data)}");
                        XYHotkeyManager.Instance.TriggerHotkey(data);
                    }
                }
            }
        }
    }
}