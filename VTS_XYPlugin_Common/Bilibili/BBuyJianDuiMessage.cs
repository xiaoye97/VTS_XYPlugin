namespace VTS_XYPlugin_Common
{
    [System.Serializable]
    public class BBuyJianDuiMessage : XYMessage
    {
        public string 用户ID;
        public string 用户名;

        /// <summary>
        /// 此处应该是此用户当前的舰队类型，而不是此次购买的类型
        /// </summary>
        public BJianDuiType 舰长类型;

        public BJianDuiType 开通类型;
        public int 开通数量;

        public BBuyJianDuiMessage() : base()
        {
        }

        public BBuyJianDuiMessage(string[] data) : base()
        {
            用户ID = data[1];
            用户名 = data[2];
            舰长类型 = data[3].ToJianDuiType();
            if (data[4] == "舰长")
            {
                开通类型 = BJianDuiType.舰长;
            }
            else if (data[4] == "提督")
            {
                开通类型 = BJianDuiType.提督;
            }
            else if (data[4] == "总督")
            {
                开通类型 = BJianDuiType.总督;
            }
            开通数量 = data[5].ToInt();
        }
    }
}