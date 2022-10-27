namespace VTS_XYPlugin_Common
{
    [System.Serializable]
    public class BSCMessage : XYMessage
    {
        public string 用户ID;
        public string 用户名;
        public int 金额;
        public int 持续时间;
        public string SC;

        public BSCMessage() : base()
        {
        }

        public BSCMessage(string[] data) : base()
        {
            用户ID = data[1];
            用户名 = data[2];
            金额 = data[3].ToInt();
            持续时间 = data[4].ToInt();
            SC = data[5];
        }
    }
}