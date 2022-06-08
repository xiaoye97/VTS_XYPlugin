namespace VTS_XYPlugin_Common
{
    [System.Serializable]
    public class BDanMuMessage : XYMessage
    {
        public int 用户ID;
        public string 用户名;
        public bool 是否房管;
        public BJianDuiType 舰队类型;
        public string 粉丝牌名称;
        public int 粉丝牌等级;
        public string 弹幕;

        public BDanMuMessage() : base()
        {
        }

        public BDanMuMessage(string[] data) : base()
        {
            用户ID = data[1].ToInt();
            用户名 = data[2];
            是否房管 = data[3].ToBool();
            舰队类型 = data[4].ToJianDuiType();
            粉丝牌名称 = data[5];
            粉丝牌等级 = data[6].ToInt();
            弹幕 = data[7];
        }
    }
}