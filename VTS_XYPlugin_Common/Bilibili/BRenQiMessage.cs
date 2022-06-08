namespace VTS_XYPlugin_Common
{
    [System.Serializable]
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