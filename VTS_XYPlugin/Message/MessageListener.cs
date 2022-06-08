using System;

namespace VTS_XYPlugin
{
    public class MessageListener
    {
        public int ID;
        public string MessageName;
        public Action<object> CallbackAction;

        public void UnRegister()
        {
            MessageCenter.Instance.UnRegister(ID);
        }
    }
}