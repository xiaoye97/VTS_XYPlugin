using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTS_XYPlugin_Common;

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
