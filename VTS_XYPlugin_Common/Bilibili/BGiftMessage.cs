namespace VTS_XYPlugin_Common
{
    [System.Serializable]
    public class BGiftMessage : XYMessage
    {
        public string 用户ID;
        public string 用户名;
        public string 礼物名;
        public int 礼物数量;
        public BGiftCoinType 瓜子类型;
        public int 瓜子数量;
        public string 头像图片链接;

        public BGiftMessage() : base()
        {
        }

        public BGiftMessage(string[] data) : base()
        {
            用户ID = data[1];
            用户名 = data[2];
            礼物名 = data[3];
            礼物数量 = data[4].ToInt();
            瓜子类型 = data[5].ToGiftCoinType();
            瓜子数量 = data[6].ToInt();
            头像图片链接 = data[7];
        }
    }
}