using VTS_XYPlugin_Common;

namespace VTS_XYPlugin
{
    /// <summary>
    /// 礼物掉落脚本
    /// </summary>
    public class XYDropItemBehaviour : XYCustomBehaviour
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
            var dict = XYDropManager.Instance.SearchDropItemByTriggerGift(message.礼物名);
            foreach (var kv in dict)
            {
                var data = kv.Value;
                if (message.礼物数量 >= data.TriggerCount)
                {
                    XYDropManager.Instance.StartDrop(data, message.礼物数量, message.用户ID);
                }
            }
        }

        public virtual void 当收到舰长(object obj)
        {
            BBuyJianDuiMessage message = (BBuyJianDuiMessage)obj;
            var dict = XYDropManager.Instance.SearchDropItemByTriggerGift(message.舰长类型.ToString());
            foreach (var kv in dict)
            {
                var data = kv.Value;
                XYDropManager.Instance.StartDrop(data, 1, message.用户ID);
            }
        }

        public virtual void 当收到SC(object obj)
        {
            //BSCMessage message = (BSCMessage)obj;
        }
    }
}