using System;
using UnityEngine;
using Newtonsoft.Json;
using VTS_XYPlugin_Common;
using System.Collections.Generic;

namespace VTS_XYPlugin
{
    /// <summary>
    /// 消息中心，由此处中转各种消息
    /// </summary>
    public class MessageCenter : Singleton<MessageCenter>
    {
        public static bool RunMessageCenter = true;
        private List<MessageListener> messageListeners = new List<MessageListener>();
        private int listenerIDCur;

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        public void Send(XYMessage message)
        {
            if (!RunMessageCenter) return;
            if (message.LogMessage)
            {
                XYLog.LogMessage($"[{message.消息名}]{JsonConvert.SerializeObject(message)}");
            }
            for (int i = messageListeners.Count - 1; i >= 0; i--)
            {
                var listener = messageListeners[i];
                if (listener != null)
                {
                    if (listener.MessageName == message.消息名)
                    {
                        if (listener.CallbackAction != null)
                        {
                            try
                            {
                                //XYLog.LogMessage($"ID为{listener.ID}的监听器收到{listener.MessageName}");
                                listener.CallbackAction(message);
                            }
                            catch (Exception ex)
                            {
                                XYLog.LogError($"{ex}");
                            }
                        }
                    }
                }
                else
                {
                    messageListeners.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        public MessageListener Register<T>(Action<object> callback) where T : XYMessage
        {
            MessageListener listener = new MessageListener();
            listener.ID = listenerIDCur++;
            listener.MessageName = typeof(T).Name;
            listener.CallbackAction = callback;
            messageListeners.Add(listener);
            XYLog.LogMessage($"注册了消息接收器:{typeof(T).Name} ID:{listener.ID}");
            return listener;
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="id"></param>
        public void UnRegister(int id)
        {
            for (int i = messageListeners.Count - 1; i >= 0; i--)
            {
                var listener = messageListeners[i];
                if (listener != null)
                {
                    if (listener.ID == id)
                    {
                        messageListeners.RemoveAt(i);
                        return;
                    }
                }
                else
                {
                    messageListeners.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 全部取消订阅
        /// </summary>
        public void UnRegisterAll()
        {
            messageListeners.Clear();
        }
    }
}
