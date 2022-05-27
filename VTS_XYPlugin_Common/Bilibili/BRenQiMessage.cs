namespace VTS_XYPlugin_Common
{
    [System.Serializable]
    /// <summary>
    /// 人气消息数据
    /// </summary>
    public class BRenQiMessage : XYMessage
    {
        public int 人气;

        public BRenQiMessage() : base()
        {
        }

        public BRenQiMessage(string[] data) : base()
        {
            人气 = data[1].ToInt();
        }
    }
}
